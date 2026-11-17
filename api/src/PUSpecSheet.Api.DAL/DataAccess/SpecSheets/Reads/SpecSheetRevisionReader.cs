using Microsoft.EntityFrameworkCore;
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
/// given id, as its sheet stood when the version was published. It builds the same <see cref="DbSpecSheet"/> a copy is
/// read into and maps it the same way, so everything after the read is the same whichever it came from.
/// </summary>
/// <remarks>
/// The sheet is read in one split query, like a copy: filtered includes bring back only the tables, groups and rows
/// shown at the version's moment and the revisions that show them, the values of those row revisions alone, and each
/// cell with its template cell, type and configuration. Tables, groups, rows and cells have the ids of their spec
/// items; widgets, notes and workflow step actors, which the revisions keep inside their items, are numbered from 1
/// within the sheet. An item's creator is who wrote the revision shown, as the revisions don't keep it per item.
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
        var sheet = await ReadSheetAsync(version.SheetId, version.PublishedDateTime, cancellationToken);
        var submissions = await ReadSubmissionsAsync(version, found.PreviousPublishedDateTime, cancellationToken);

        var createdByUserId = version.SourceCreatedByUserId ?? version.PublishedByUserId;
        var noteAuthorIds = sheet.Tables
            .SelectMany(table => table.Revisions)
            .SelectMany(revision => revision.Notes ?? [])
            .Select(note => note.CreatedByUserId);
        var users = await ReadUsersAsync(noteAuthorIds.Append(createdByUserId).Append(version.PublishedByUserId), cancellationToken);

        var ids = new Numbering();
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
            Tables = [.. sheet.Tables.Select(table => MapTable(table, specSheetId, ids, users)).OrderBy(table => table.DisplayOrder)],
            PropertyValues = [.. sheet.Properties.Select(property => MapProperty(property, specSheetId))],
            WorkflowSteps = [.. submissions.Select(submission => MapSubmission(submission, specSheetId, ids))],
            ExternalReferences = [.. sheet.ExternalReferences
                .Where(reference => reference.Revisions.Count > 0)
                .Select(reference => MapExternalReference(reference, specSheetId))]
        };

        return dbSpecSheet.MapToDomain();
    }

    // The sheet with what was shown at the moment: each table, group, row and property whose revision then doesn't
    // remove it, with that revision alone, and each external reference with its revision then, if it had one. Groups
    // and rows are only read under tables and groups that are shown, so nothing in a removed table is.
    private async Task<DbSheet> ReadSheetAsync(int sheetId, DateTime moment, CancellationToken cancellationToken)
    {
        return await _dbContext.Sheets
            .Where(s => s.Id == sheetId)
            .Include(s => s.SpecSheetPhase)
            .Include(s => s.SpecSheetType)

            .Include(s => s.Tables.Where(t => t.Revisions.Any(r => r.Status == RevisionStatus.Published
                    && !r.IsDeleted
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
                .ThenInclude(t => t.Revisions.Where(r => r.Status == RevisionStatus.Published
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment)))

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups.Where(g => g.Revisions.Any(r => r.Status == RevisionStatus.Published
                    && !r.IsDeleted
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
                    .ThenInclude(g => g.Revisions.Where(r => r.Status == RevisionStatus.Published
                        && r.PublishedDateTime <= moment
                        && (r.SupersededDateTime == null || r.SupersededDateTime > moment)))

            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Rows.Where(row => row.Revisions.Any(r => r.Status == RevisionStatus.Published
                        && !r.IsDeleted
                        && r.PublishedDateTime <= moment
                        && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
                        .ThenInclude(row => row.Revisions.Where(r => r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment)))
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

            .Include(s => s.Properties.Where(p => p.Revisions.Any(r => r.Status == RevisionStatus.Published
                    && !r.IsDeleted
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
                .ThenInclude(p => p.Revisions.Where(r => r.Status == RevisionStatus.Published
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment)))
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

    private static DbSpecSheetTable MapTable(DbSheetTable table, int specSheetId, Numbering ids, Dictionary<int, DbUser> users)
    {
        var revision = table.Revisions.Single();

        // A group is nested under the group it is linked to, which the revisions keep by ReferenceID.
        var groupIdsByReference = table.Groups
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
            LastModifiedByUserId = revision.RevisionNumber > 1 ? revision.CreatedByUserId : null,
            LastModifiedDateTime = revision.RevisionNumber > 1 ? revision.PublishedDateTime : null,
            ConfigurationOverride = revision.ConfigurationOverride,
            Groups = [.. table.Groups.Select(group => MapGroup(group, groupIdsByReference, ids))],
            Widgets = [.. (revision.WidgetValues ?? []).Select(widget =>
            {
                var widgetId = ids.Next("table widget");
                return new DbSpecSheetTableWidget
                {
                    Id = widgetId,
                    SpecSheetTableId = table.Id,
                    TableTemplateWidgetId = widget.TemplateWidgetId,
                    Value = new DbTableWidgetValue { Id = widgetId, SpecSheetTableWidgetId = widgetId, Value = widget.Value }
                };
            })],
            Notes = [.. (revision.Notes ?? []).Select(note => new DbSpecSheetTableNote
            {
                Id = ids.Next("note"),
                SpecSheetTableId = table.Id,
                Note = note.Note,
                SourceSystem = note.SourceSystem,
                IsObsolete = note.IsObsolete,
                CreatedByUserId = note.CreatedByUserId,
                CreatedDateTime = note.CreatedDateTime,
                CreatedByUser = users.GetValueOrDefault(note.CreatedByUserId)
            })]
        };
    }

    private static DbSpecSheetTableGroup MapGroup(DbSheetTableGroup group, Dictionary<Guid, int> groupIdsByReference, Numbering ids)
    {
        var revision = group.Revisions.Single();
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
            Rows = [.. group.Rows.Select(MapRow).OrderBy(row => row.RowNumber)],
            Widgets = [.. (revision.WidgetValues ?? []).Select(widget =>
            {
                var widgetId = ids.Next("group widget");
                return new DbSpecSheetTableGroupWidget
                {
                    Id = widgetId,
                    SpecSheetTableGroupId = group.Id,
                    TableGroupTemplateWidgetId = widget.TemplateWidgetId,
                    Value = new DbTableGroupWidgetValue { Id = widgetId, SpecSheetTableGroupWidgetId = widgetId, Value = widget.Value }
                };
            })],
            CreatedByUserId = revision.CreatedByUserId,
            CreatedDateTime = group.CreatedDateTime,
            LastModifiedByUserId = revision.RevisionNumber > 1 ? revision.CreatedByUserId : null,
            LastModifiedDateTime = revision.RevisionNumber > 1 ? revision.PublishedDateTime : null
        };
    }

    private static DbSpecSheetTableRow MapRow(DbSheetTableRow row)
    {
        var revision = row.Revisions.Single();
        var texts = revision.TextValues.ToDictionary(value => value.SheetTableCellId);
        var numbers = revision.NumericValues.ToDictionary(value => value.SheetTableCellId);
        var pairs = revision.PairValues.ToDictionary(value => value.SheetTableCellId);
        var links = revision.LinkedSheetValues.ToDictionary(value => value.SheetTableCellId);
        var overrides = revision.CellConfigurationOverrides.ToDictionary(value => value.SheetTableCellId);

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
            LastModifiedByUserId = revision.RevisionNumber > 1 ? revision.CreatedByUserId : null,
            LastModifiedDateTime = revision.RevisionNumber > 1 ? revision.PublishedDateTime : null
        };

        foreach (var cell in row.Cells.OrderBy(cell => cell.TableCellTemplate!.ColumnNumber))
        {
            var templateCell = cell.TableCellTemplate!;
            var mappedCell = new DbSpecSheetTableCell
            {
                Id = cell.Id,
                SpecSheetTableRowId = row.Id,
                TableCellTypeId = templateCell.TableCellTypeId,
                CellConfigurationId = templateCell.CellConfigurationId,
                RowNumber = mapped.RowNumber,
                ColumnNumber = templateCell.ColumnNumber,
                ColSpan = templateCell.ColSpan,
                RowSpan = templateCell.RowSpan,
                ReferenceId = cell.ReferenceId,
                IsDeleted = false,
                CreatedByUserId = mapped.CreatedByUserId,
                CreatedDateTime = cell.CreatedDateTime,
                LastModifiedByUserId = mapped.LastModifiedByUserId,
                LastModifiedDateTime = mapped.LastModifiedDateTime,
                ConfigurationOverride = overrides.GetValueOrDefault(cell.Id)?.ConfigurationOverride,
                TableCellType = templateCell.TableCellType,
                CellConfiguration = templateCell.CellConfiguration
            };

            // A copied cell always has a value row; where the revision holds no value, it is an empty one of the kind
            // the cell's type holds, as the app gives a new cell.
            if (numbers.TryGetValue(cell.Id, out var number))
            {
                mappedCell.NumericValue = new DbNumericValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = number.Value };
            }
            else if (texts.TryGetValue(cell.Id, out var text))
            {
                mappedCell.TextValue = new DbTextValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = text.Value };
            }
            else if (pairs.TryGetValue(cell.Id, out var pair))
            {
                mappedCell.PairValue = new DbPairValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, PairValueX = pair.PairValueX, PairValueY = pair.PairValueY };
            }
            else if (links.TryGetValue(cell.Id, out var link))
            {
                mappedCell.LinkedSpecSheetValue = LinkedValue(cell.Id, link.TableReferenceId, link.LinkedSpecSheetTypeId, link.LinkedSpecSheetPhaseId, link.LinkedReferenceId);
            }
            else
            {
                SetEmptyValue(mappedCell, templateCell.TableCellType!.ModelType);
            }

            mapped.Cells.Add(mappedCell);
        }

        return mapped;
    }

    private static void SetEmptyValue(DbSpecSheetTableCell cell, string modelType)
    {
        switch (modelType)
        {
            case "numeric":
            case "numericDropdown":
                cell.NumericValue = new DbNumericValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = null };
                break;
            case "pair":
                cell.PairValue = new DbPairValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, PairValueX = null, PairValueY = null };
                break;
            case "linkedSpecSheet":
                cell.LinkedSpecSheetValue = LinkedValue(cell.Id, null, null, null, null);
                break;
            default:
                cell.TextValue = new DbTextValue { Id = cell.Id, SpecSheetTableCellId = cell.Id, Value = null };
                break;
        }
    }

    private static DbLinkedSpecSheetValue LinkedValue(int cellId, Guid? tableReferenceId, int? typeId, int? phaseId, Guid? linkedReferenceId)
    {
        return new DbLinkedSpecSheetValue
        {
            Id = cellId,
            SpecSheetTableCellId = cellId,
            TableReferenceId = tableReferenceId,
            LinkedSpecSheetTypeId = typeId,
            LinkedSpecSheetPhaseId = phaseId,
            LinkedReferenceId = linkedReferenceId
        };
    }

    private static DbSpecSheetPropertyValue MapProperty(DbSheetProperty property, int specSheetId)
    {
        return new DbSpecSheetPropertyValue
        {
            Id = property.Id,
            SpecSheetId = specSheetId,
            SpecSheetTypePropertyId = property.SpecSheetTypePropertyId,
            PropertyValue = property.Revisions.Single().PropertyValue,
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
    private static DbWorkflowStep MapSubmission(DbSheetSubmission submission, int specSheetId, Numbering ids)
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

    // Ids for what the revisions keep inside their items, numbered from 1 within the sheet for each kind.
    private sealed class Numbering
    {
        private readonly Dictionary<string, int> _last = [];

        public int Next(string kind)
        {
            _last[kind] = _last.GetValueOrDefault(kind) + 1;
            return _last[kind];
        }
    }
}
