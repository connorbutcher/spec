using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Mappers.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Access;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Revisions;

/// <summary>
/// Reads a published spec sheet from the revisions: the version that was migrated from the SpecSheets copy with the
/// given id, as its sheet stood when the version was published, or the sheet's drafts
/// (<see cref="ReadDraftAsync"/>). It builds the same <see cref="DbSpecSheet"/> a copy is
/// read into and maps it the same way, so everything after the read is the same whichever it came from.
/// </summary>
/// <remarks>
/// The sheet is read in one split query, like a copy: filtered includes bring back only the tables, groups and rows
/// shown at the version's moment and the revisions that show them, the values of those row revisions alone, and each
/// cell with its template cell, type and configuration; and the widget and note rows of those table and group
/// revisions with their items. Tables, groups, rows, cells, widgets and notes have the ids of their spec items;
/// workflow step actors, which the revisions don't keep as items, are numbered from 1 within the sheet. An item's
/// creator is who wrote the revision shown, as the revisions don't keep it per item, except a note's.
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

        var actorIds = new RevisionItemNumbering();
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
            Tables = SpecSheetRevisionMapper.MapTables(sheet, specSheetId, users),
            PropertyValues = SpecSheetRevisionMapper.MapProperties(sheet, specSheetId),
            WorkflowSteps = [.. submissions.Select(submission => SpecSheetRevisionMapper.MapSubmission(submission, specSheetId, actorIds))],
            ExternalReferences = SpecSheetRevisionMapper.MapExternalReferences(sheet, specSheetId)
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
            Tables = SpecSheetRevisionMapper.MapTables(sheet, specSheetId, users),
            PropertyValues = SpecSheetRevisionMapper.MapProperties(sheet, specSheetId),
            ExternalReferences = SpecSheetRevisionMapper.MapExternalReferences(sheet, specSheetId)
        };

        return dbSpecSheet.MapToDomain();
    }

    // The sheet with what was shown at the moment: each table, group, row and property whose revision then doesn't
    // remove it, with that revision alone, and each external reference with its revision then, if it had one. Groups
    // and rows are only read under tables and groups that are shown, so nothing in a removed table is. With drafts,
    // each item's draft is read too, and is the revision shown where it has one (see SpecSheetRevisionMapper).
    internal async Task<DbSheet> ReadSheetAsync(int sheetId, DateTime moment, bool withDrafts, CancellationToken cancellationToken)
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

            .Include(s => s.Tables)
                .ThenInclude(t => t.Revisions)
                    .ThenInclude(r => r.WidgetValues)
                        .ThenInclude(v => v.SheetTableWidget)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Revisions)
                    .ThenInclude(r => r.NoteValues)
                        .ThenInclude(v => v.SheetTableNote)
            .Include(s => s.Tables)
                .ThenInclude(t => t.Groups)
                    .ThenInclude(g => g.Revisions)
                        .ThenInclude(r => r.WidgetValues)
                            .ThenInclude(v => v.SheetTableGroupWidget)

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

    internal async Task<Dictionary<int, DbUser>> ReadUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken)
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
            .SelectMany(revision => revision.NoteValues)
            .Select(note => note.SheetTableNote!.CreatedByUserId);
    }
}
