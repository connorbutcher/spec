using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.SpecSheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.References;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Values;
using PUSpecSheet.Api.Database.Schema.Workflow;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

/// <summary>
/// Reads a published spec sheet from the revisions: the version that was migrated from the SpecSheets copy with the
/// given id, as its sheet stood when the version was published, or the drafts of a sheet type edited on the revisions
/// (<see cref="ReadDraftAsync"/>). It builds the same <see cref="DbSpecSheet"/> a copy is
/// read into and maps it the same way, so everything after the read is the same whichever it came from.
/// </summary>
/// <remarks>
/// The sheet is read in one split query, like a copy: filtered includes bring back only the tables, groups and rows
/// shown at the version's moment and the revisions that show them, the values of those row revisions alone, and each
/// cell with its template cell, type and configuration. Tables, groups, rows and cells have the ids of their spec
/// items; widgets, notes and workflow step actors, which the revisions keep inside their items, are numbered from 1
/// within the sheet, in the order of their tables' and groups' ids (<see cref="Processors.Drafts.Base.DraftItemNumbers"/> finds them again for
/// an edit). An item's creator is who wrote the revision shown, as the revisions don't keep it per item.
/// </remarks>
internal class SpecSheetRevisionReader
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SpecSheetRevisionReader(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    /// <summary>
    /// The published spec sheet, or null when the copy with this id hasn't been migrated to a version.
    /// </summary>
    public async Task<SpecSheet?> ReadPublishedAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var found = await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(v => v.SourceSpecSheetId == specSheetId)
            .OrderByDescending(v => v.Id)
            .Select(v => new
            {
                Version = v,
                PreviousPublishedDateTime = _dbContext.SheetVersions
                    .Where(previous => previous.SheetId == v.SheetId && previous.VersionNumber < v.VersionNumber)
                    .OrderByDescending(previous => previous.VersionNumber)
                    .Select(previous => (DateTime?)previous.PublishedDateTime)
                    .FirstOrDefault()
            })
            .FirstOrDefaultAsync(cancellationToken);
        if (found is null)
        {
            return null;
        }

        var version = found.Version;
        var sheet = await ReadSheetAsync(version.SheetId, version.PublishedDateTime, false, cancellationToken);
        var submissions = await ReadSubmissionsAsync(version, found.PreviousPublishedDateTime, cancellationToken);

        var createdByUserId = version.SourceCreatedByUserId ?? version.PublishedByUserId;
        var users = await ReadUsersAsync(NoteAuthorIds(sheet).Append(createdByUserId).Append(version.PublishedByUserId), cancellationToken);

        var ids = new RevisionItemNumbering();
        var dbSpecSheet = new DbSpecSheet
        {
            Id = specSheetId,
            LegacySpecSheetId = version.LegacySpecSheetId,
            SpecSheetPhaseId = sheet.SpecSheetPhaseId,
            SpecSheetTypeId = sheet.SpecSheetTypeId,
            IsPublished = true,
            CreatedByUserId = createdByUserId,
            CreatedDateTime = version.SourceCreatedDateTime ?? version.PublishedDateTime,
            HasBeenMigrated = version.HasBeenMigrated,
            CreatedByUser = users.GetValueOrDefault(createdByUserId),
            PublishedByUserId = version.PublishedByUserId,
            PublishedDateTime = version.PublishedDateTime,
            ChangeLog = version.ChangeLog,
            PublishedByUser = users.GetValueOrDefault(version.PublishedByUserId),
            SpecSheetPhase = sheet.SpecSheetPhase,
            SpecSheetType = sheet.SpecSheetType,
            Tables = MapTables(sheet, specSheetId, ids, users),
            PropertyValues = MapProperties(sheet, specSheetId),
            WorkflowSteps = [.. submissions.Select(submission => MapSubmission(submission, specSheetId, ids))],
            ExternalReferences = MapExternalReferences(sheet, specSheetId)
        };

        return dbSpecSheet.MapToDomain();
    }

    /// <summary>
    /// The unpublished spec sheet with this id as its drafts have it: each item's draft where it has one, otherwise its
    /// current published revision. The spec sheet's own row gives what isn't kept in the revisions: who created it and
    /// when, and its workflow steps.
    /// </summary>
    public async Task<SpecSheet> ReadDraftAsync(int specSheetId, int sheetId, CancellationToken cancellationToken)
    {
        var header = await _dbContext.SpecSheets
            .Where(s => s.Id == specSheetId)
            .Include(s => s.CreatedByUser)
            .Include(s => s.WorkflowSteps)
                .ThenInclude(step => step.Actors)
                    .ThenInclude(actor => actor.ActorUser)
            .AsSplitQuery()
            .AsNoTracking()
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new KeyNotFoundException($"Spec Sheet with Id {specSheetId} not found.");

        var sheet = await ReadSheetAsync(sheetId, DateTime.UtcNow, true, cancellationToken);
        var users = await ReadUsersAsync(NoteAuthorIds(sheet), cancellationToken);

        var dbSpecSheet = header with
        {
            SpecSheetPhase = sheet.SpecSheetPhase,
            SpecSheetType = sheet.SpecSheetType,
            Tables = MapTables(sheet, specSheetId, new RevisionItemNumbering(), users),
            PropertyValues = MapProperties(sheet, specSheetId),
            ExternalReferences = MapExternalReferences(sheet, specSheetId)
        };

        return dbSpecSheet.MapToDomain();
    }

    // The sheet with what was shown at the moment: each table, group, row and property whose revision then doesn't
    // remove it, with that revision alone, and each external reference with its revision then, if it had one. Groups
    // and rows are only read under tables and groups that are shown, so nothing in a removed table is. With drafts,
    // each item's draft is read too, and is the revision shown where it has one (see Shown).
    private async Task<DbSheet> ReadSheetAsync(int sheetId, DateTime moment, bool withDrafts, CancellationToken cancellationToken)
    {
        return await _dbContext.Sheets
            .Where(s => s.Id == sheetId)
            .Include(s => s.SpecSheetPhase)
            .Include(s => s.SpecSheetType)

            .Include(s => s.Tables.Where(t => t.Revisions.Any(r => !r.IsDeleted
                    && ((withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                .ThenInclude(t => t.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                    || (r.Status == RevisionStatus.Published
                        && r.PublishedDateTime <= moment
                        && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups.Where(g => g.Revisions.Any(r => !r.IsDeleted
                    && ((withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                    .ThenInclude(g => g.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows.Where(row => row.Revisions.Any(r => !r.IsDeleted
                        && ((withDrafts && r.Status == RevisionStatus.Draft)
                            || (r.Status == RevisionStatus.Published
                                && r.PublishedDateTime <= moment
                                && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                        .ThenInclude(row => row.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                            || (r.Status == RevisionStatus.Published
                                && r.PublishedDateTime <= moment
                                && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
                            .ThenInclude(r => r.TextValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.NumericValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.PairValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.LinkedSheetValues)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Revisions)
                            .ThenInclude(r => r.CellConfigurationOverrides)

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Cells)
                            .ThenInclude(c => c.TableCellTemplate)
                                .ThenInclude(tc => tc!.TableCellType)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows)
                        .ThenInclude(row => row.Cells)
                            .ThenInclude(c => c.TableCellTemplate)
                                .ThenInclude(tc => tc!.CellConfiguration)

            .Include(s => s.Properties.Where(p => p.Revisions.Any(r => !r.IsDeleted
                    && ((withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))))
                .ThenInclude(p => p.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                    || (r.Status == RevisionStatus.Published
                        && r.PublishedDateTime <= moment
                        && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
            .Include(s => s.Properties)
                .ThenInclude(p => p.SpecSheetTypeProperty)

            .Include(s => s.ExternalReferences)
                .ThenInclude(e => e.Revisions.Where(r => !r.IsDeleted
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment)))

            .AsSplitQuery()
            .AsNoTracking()
            .FirstAsync(cancellationToken);
    }

    // A version's approval history: the submission that published it, and those made since the version before it,
    // as the copy's workflow steps were.
    private async Task<List<DbSheetSubmission>> ReadSubmissionsAsync(DbSheetVersion version, DateTime? previousPublishedDateTime, CancellationToken cancellationToken)
    {
        return await _dbContext.SheetSubmissions
            .Where(submission => submission.SheetId == version.SheetId
                && (submission.SheetVersionId == version.Id
                    || (submission.SheetVersionId == null
                        && submission.SubmittedDateTime <= version.PublishedDateTime
                        && (previousPublishedDateTime == null || submission.SubmittedDateTime > previousPublishedDateTime))))
            .Include(submission => submission.Approvers)
                .ThenInclude(approver => approver.User)
            .OrderBy(submission => submission.SubmittedDateTime)
            .ThenBy(submission => submission.Id)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    private async Task<Dictionary<int, DbUser>> ReadUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken)
    {
        var wanted = userIds.Distinct().ToList();
        return await _dbContext.Users
            .AsNoTracking()
            .Where(user => wanted.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, cancellationToken);
    }

    private static IEnumerable<int> NoteAuthorIds(DbSheet sheet)
    {
        return sheet.Tables
            .SelectMany(table => table.Revisions)
            .SelectMany(revision => revision.Notes ?? [])
            .Select(note => note.CreatedByUserId);
    }

    // Of an item's revisions read, the one shown: its draft where it has one, otherwise its published revision.
    private static TRevision Shown<TRevision>(ICollection<TRevision> revisions)
        where TRevision : IDbSheetRevision
    {
        return revisions.OrderBy(revision => revision.Status == RevisionStatus.Draft ? 0 : 1).First();
    }

    private static bool IsShown<TRevision>(ICollection<TRevision> revisions)
        where TRevision : IDbSheetRevision
    {
        return !Shown(revisions).IsDeleted;
    }

    private static int? LastModifiedByUserId(IDbSheetRevision revision)
    {
        return revision.RevisionNumber > 1 ? revision.CreatedByUserId : null;
    }

    private static DateTime? LastModifiedDateTime(IDbSheetRevision revision)
    {
        return revision.RevisionNumber > 1 ? revision.PublishedDateTime ?? revision.LastModifiedDateTime : null;
    }

    private static List<DbSpecSheetTable> MapTables(DbSheet sheet, int specSheetId, RevisionItemNumbering ids, Dictionary<int, DbUser> users)
    {
        return [.. sheet.Tables
            .Where(table => IsShown(table.Revisions))
            .OrderBy(table => table.Id)
            .Select(table => MapTable(table, specSheetId, ids, users))
            .OrderBy(table => table.DisplayOrder)];
    }

    private static List<DbSpecSheetPropertyValue> MapProperties(DbSheet sheet, int specSheetId)
    {
        return [.. sheet.Properties
            .Where(property => IsShown(property.Revisions))
            .Select(property => MapProperty(property, specSheetId))];
    }

    private static List<DbSpecSheetExternalReference> MapExternalReferences(DbSheet sheet, int specSheetId)
    {
        return [.. sheet.ExternalReferences
            .Where(reference => reference.Revisions.Count > 0)
            .Select(reference => MapExternalReference(reference, specSheetId))];
    }

    private static DbSpecSheetTable MapTable(DbSheetTable table, int specSheetId, RevisionItemNumbering ids, Dictionary<int, DbUser> users)
    {
        var revision = Shown(table.Revisions);

        // A group is nested under the group it is linked to, which the revisions keep by ReferenceID.
        var groupIdsByReference = table.Groups
            .Where(group => IsShown(group.Revisions))
            .GroupBy(group => group.ReferenceId)
            .ToDictionary(group => group.Key, group => group.First().Id);

        return new DbSpecSheetTable
        {
            Id = table.Id,
            SpecSheetId = specSheetId,
            TableTemplateId = table.TableTemplateId,
            Title = revision.Title,
            DisplayOrder = revision.DisplayOrder,
            ReferenceId = table.ReferenceId,
            IsDeleted = false,
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = table.CreatedDateTime,
            LastModifiedByUserId = LastModifiedByUserId(revision),
            LastModifiedDateTime = LastModifiedDateTime(revision),
            ConfigurationOverride = revision.ConfigurationOverride,
            Groups = [.. table.Groups.Where(group => IsShown(group.Revisions)).OrderBy(group => group.Id).Select(group => MapGroup(group, groupIdsByReference, ids))],
            Widgets = [.. (revision.WidgetValues ?? []).Select(widget => MapTableWidget(widget, table.Id, ids))],
            Notes = [.. (revision.Notes ?? []).Select(note => MapNote(note, table.Id, ids, users))]
        };
    }

    private static DbSpecSheetTableWidget MapTableWidget(SheetWidgetValue widget, int tableId, RevisionItemNumbering ids)
    {
        var widgetId = ids.Next("table widget");
        return new DbSpecSheetTableWidget
        {
            Id = widgetId,
            SpecSheetTableId = tableId,
            TableTemplateWidgetId = widget.TemplateWidgetId,
            Value = new DbTableWidgetValue { Id = widgetId, SpecSheetTableWidgetId = widgetId, Value = widget.Value }
        };
    }

    private static DbSpecSheetTableNote MapNote(SheetTableNote note, int tableId, RevisionItemNumbering ids, Dictionary<int, DbUser> users)
    {
        return new DbSpecSheetTableNote
        {
            Id = ids.Next("note"),
            SpecSheetTableId = tableId,
            Note = note.Note,
            SourceSystem = note.SourceSystem,
            IsObsolete = note.IsObsolete,
            CreatedByUserId = note.CreatedByUserId,
            CreatedDateTime = note.CreatedDateTime,
            CreatedByUser = users.GetValueOrDefault(note.CreatedByUserId)
        };
    }

    private static DbSpecSheetTableGroup MapGroup(DbSheetTableGroup group, Dictionary<Guid, int> groupIdsByReference, RevisionItemNumbering ids)
    {
        var revision = Shown(group.Revisions);
        int? linkedGroupId = revision.LinkedSheetTableGroupReferenceId is { } linkedReference
            && groupIdsByReference.TryGetValue(linkedReference, out var linked)
                ? linked
                : null;

        return new DbSpecSheetTableGroup
        {
            Id = group.Id,
            SpecSheetTableId = group.SheetTableId,
            TableGroupTemplateId = group.TableGroupTemplateId,
            LinkedSpecSheetTableGroupId = linkedGroupId,
            Order = revision.DisplayOrder,
            ReferenceId = group.ReferenceId,
            IsDeleted = false,
            Rows = [.. group.Rows.Where(row => IsShown(row.Revisions)).Select(MapRow).OrderBy(row => row.RowNumber)],
            Widgets = [.. (revision.WidgetValues ?? []).Select(widget => MapGroupWidget(widget, group.Id, ids))],
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = group.CreatedDateTime,
            LastModifiedByUserId = LastModifiedByUserId(revision),
            LastModifiedDateTime = LastModifiedDateTime(revision)
        };
    }

    private static DbSpecSheetTableGroupWidget MapGroupWidget(SheetWidgetValue widget, int groupId, RevisionItemNumbering ids)
    {
        var widgetId = ids.Next("group widget");
        return new DbSpecSheetTableGroupWidget
        {
            Id = widgetId,
            SpecSheetTableGroupId = groupId,
            TableGroupTemplateWidgetId = widget.TemplateWidgetId,
            Value = new DbTableGroupWidgetValue { Id = widgetId, SpecSheetTableGroupWidgetId = widgetId, Value = widget.Value }
        };
    }

    private static DbSpecSheetTableRow MapRow(DbSheetTableRow row)
    {
        var revision = Shown(row.Revisions);
        var values = new RowRevisionValues(revision);

        var mapped = new DbSpecSheetTableRow
        {
            Id = row.Id,
            SpecSheetTableGroupId = row.SheetTableGroupId,
            TableRowTemplateId = row.TableRowTemplateId,
            RowNumber = revision.DisplayOrder,
            ReferenceId = row.ReferenceId,
            IsDeleted = false,
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = row.CreatedDateTime,
            LastModifiedByUserId = LastModifiedByUserId(revision),
            LastModifiedDateTime = LastModifiedDateTime(revision)
        };

        foreach (var cell in row.Cells.OrderBy(cell => cell.TableCellTemplate!.ColumnNumber))
        {
            mapped.Cells.Add(MapCell(cell, mapped, values));
        }

        return mapped;
    }

    // A cell takes its place, type and configuration from its template cell, and who made and changed it from its row.
    private static DbSpecSheetTableCell MapCell(DbSheetTableCell cell, DbSpecSheetTableRow row, RowRevisionValues values)
    {
        var templateCell = cell.TableCellTemplate!;
        var mapped = new DbSpecSheetTableCell
        {
            Id = cell.Id,
            SpecSheetTableRowId = row.Id,
            TableCellTypeId = templateCell.TableCellTypeId,
            CellConfigurationId = templateCell.CellConfigurationId,
            RowNumber = row.RowNumber,
            ColumnNumber = templateCell.ColumnNumber,
            ColSpan = templateCell.ColSpan,
            RowSpan = templateCell.RowSpan,
            ReferenceId = cell.ReferenceId,
            IsDeleted = false,
            CreatedByUserId = row.CreatedByUserId,
            CreatedDateTime = cell.CreatedDateTime,
            LastModifiedByUserId = row.LastModifiedByUserId,
            LastModifiedDateTime = row.LastModifiedDateTime,
            ConfigurationOverride = values.ConfigurationOverrideOf(cell.Id),
            TableCellType = templateCell.TableCellType,
            CellConfiguration = templateCell.CellConfiguration
        };

        values.SetValue(mapped, templateCell.TableCellType!.ModelType);
        return mapped;
    }

    private static DbSpecSheetPropertyValue MapProperty(DbSheetProperty property, int specSheetId)
    {
        return new DbSpecSheetPropertyValue
        {
            Id = property.Id,
            SpecSheetId = specSheetId,
            SpecSheetTypePropertyId = property.SpecSheetTypePropertyId,
            PropertyValue = Shown(property.Revisions).PropertyValue,
            SpecSheetTypeProperty = property.SpecSheetTypeProperty
        };
    }

    private static DbSpecSheetExternalReference MapExternalReference(DbSheetExternalReference reference, int specSheetId)
    {
        var revision = reference.Revisions.Single();
        return new DbSpecSheetExternalReference
        {
            Id = reference.Id,
            SpecSheetId = specSheetId,
            ExternalId = revision.ExternalId,
            ReferenceId = reference.ReferenceId,
            SystemName = revision.SystemName,
            OwnerName = revision.OwnerName,
            Url = revision.Url,
            Type = revision.Type,
            Grouping = revision.Grouping,
            SystemIconUrl = revision.SystemIconUrl,
            Description = revision.Description
        };
    }

    // A submission as the workflow step it was migrated from. A withdrawn submission was a step left pending.
    private static DbWorkflowStep MapSubmission(DbSheetSubmission submission, int specSheetId, RevisionItemNumbering ids)
    {
        return new DbWorkflowStep
        {
            Id = submission.Id,
            SpecSheetId = specSheetId,
            StartedDateTime = submission.SubmittedDateTime,
            State = submission.Status switch
            {
                SubmissionStatus.Approved => WorkflowState.Approved,
                SubmissionStatus.Rejected => WorkflowState.Rejected,
                _ => WorkflowState.Pending
            },
            Actors = [.. submission.Approvers
                .OrderBy(approver => approver.UserId)
                .Select(approver => new DbWorkflowStepActor
                {
                    Id = ids.Next("actor"),
                    WorkflowStepId = submission.Id,
                    ActorUserId = approver.UserId,
                    ActorUser = approver.User,
                    State = approver.Decision switch
                    {
                        ApproverDecision.Approved => WorkflowState.Approved,
                        ApproverDecision.Rejected => WorkflowState.Rejected,
                        _ => WorkflowState.Pending
                    },
                    Comment = approver.Reason == DbSheetSubmissionApprover.NoReasonGiven ? null : approver.Reason,
                    ActionedDateTime = approver.DecidedDateTime
                })]
        };
    }
}