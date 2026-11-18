using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Revisions;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads.Sheets;

/// <summary>
/// Reads a sheet from the revisions into the <see cref="Sheet"/> models: as it stood when one of its versions was
/// published (<see cref="ReadPublishedAsync"/>), or as it stands now with its drafts laid over its published
/// revisions (<see cref="ReadDraftAsync"/>).
/// </summary>
/// <remarks>
/// The sheet is read the way <see cref="SpecSheetRevisionReader"/> reads it, plus its linked tables and versions, and
/// mapped by <see cref="SheetMapper"/>. Nothing is numbered: every id is the id of
/// the item in the revision schema.
/// </remarks>
internal class SheetReader
{
    private readonly PUSpecSheetDbContext _dbContext;
    private readonly SpecSheetRevisionReader _revisionReader;

    public SheetReader(PUSpecSheetDbContext dbContext, SpecSheetRevisionReader revisionReader)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));
        ArgumentNullException.ThrowIfNull(revisionReader, nameof(revisionReader));

        _dbContext = dbContext;
        _revisionReader = revisionReader;
    }

    /// <summary>
    /// The sheet's versions, oldest first.
    /// </summary>
    public async Task<List<SheetVersion>> ReadVersionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        var versions = await ReadVersionRowsAsync(sheetId, cancellationToken);
        var users = await _revisionReader.ReadUsersAsync(versions.Select(version => version.PublishedByUserId), cancellationToken);

        var mapper = new SheetMapper(users.Values, versions);
        return [.. versions.Select(mapper.MapVersion)];
    }

    /// <summary>
    /// The sheet as it stood when the version was published: the latest version when none is given. Null when the sheet
    /// has no such version.
    /// </summary>
    public async Task<Sheet?> ReadPublishedAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken)
    {
        return await ReadPublishedAsync(
            sheetId,
            versions => versionNumber is null ? versions.LastOrDefault() : versions.FirstOrDefault(v => v.VersionNumber == versionNumber),
            cancellationToken);
    }

    /// <summary>
    /// The sheet as it stood at its latest version published at or before the moment: its latest version when no moment
    /// is given. Null when it has no such version.
    /// </summary>
    public async Task<Sheet?> ReadPublishedByAsync(int sheetId, DateTime? publishedBy, CancellationToken cancellationToken)
    {
        return await ReadPublishedAsync(
            sheetId,
            versions => versions.LastOrDefault(v => publishedBy is null || v.PublishedDateTime <= publishedBy),
            cancellationToken);
    }

    /// <summary>
    /// The latest published version of the sheet of the phase and type at or before the moment (the latest of all when no
    /// moment is given), as where to read it; null when there is none.
    /// </summary>
    public async Task<SheetLocation?> FindVersionAtAsync(int phaseId, int sheetTypeId, DateTime? publishedBy, CancellationToken cancellationToken)
    {
        var version = await (from v in _dbContext.SheetVersions.AsNoTracking()
                             join s in _dbContext.Sheets.AsNoTracking() on v.SheetId equals s.Id
                             where s.SpecSheetPhaseId == phaseId
                                 && s.SpecSheetTypeId == sheetTypeId
                                 && (publishedBy == null || v.PublishedDateTime <= publishedBy)
                             orderby v.PublishedDateTime descending
                             select new { v.SheetId, v.VersionNumber, v.SourceSpecSheetId })
            .FirstOrDefaultAsync(cancellationToken);

        return version is null
            ? null
            : new SheetLocation { SpecSheetId = version.SourceSpecSheetId ?? 0, SheetId = version.SheetId, VersionNumber = version.VersionNumber, IsDraft = false };
    }

    /// <summary>
    /// The cells with the reference ids, of the sheet as it stood at its latest version published at or before the
    /// moment (its latest version when no moment is given). Null when it has no such version.
    /// </summary>
    public async Task<SheetCells?> ReadCellsAsync(int sheetId, IReadOnlyCollection<Guid> referenceIds, DateTime? publishedBy, CancellationToken cancellationToken)
    {
        var sheet = await ReadPublishedByAsync(sheetId, publishedBy, cancellationToken);
        return sheet is null ? null : MapCells(sheet, referenceIds);
    }

    /// <summary>
    /// The cells with the reference ids, of the sheet as it stood when the version was published (the latest version when
    /// none is given). Null when it has no such version.
    /// </summary>
    public async Task<SheetCells?> ReadPublishedCellsAsync(int sheetId, int? versionNumber, IReadOnlyCollection<Guid> referenceIds, CancellationToken cancellationToken)
    {
        var sheet = await ReadPublishedAsync(sheetId, versionNumber, cancellationToken);
        return sheet is null ? null : MapCells(sheet, referenceIds);
    }

    /// <summary>
    /// Each table that has been published, by its reference id (the one it has in every sheet), as its most recently
    /// created table is now: without its groups, widgets and notes. Tables that are deleted now are left out.
    /// </summary>
    public async Task<List<SheetTable>> ReadUniqueTablesAsync(CancellationToken cancellationToken)
    {
        var tables = await _dbContext.SheetTables
            .AsNoTracking()
            .Where(table => table.Revisions.Any(r => r.Status == RevisionStatus.Published && r.SupersededDateTime == null && !r.IsDeleted))
            .Include(table => table.Revisions.Where(r => r.Status == RevisionStatus.Published && r.SupersededDateTime == null))
            .ToListAsync(cancellationToken);

        var unique = tables
            .GroupBy(table => table.ReferenceId)
            .Select(group => group.OrderByDescending(table => table.CreatedDateTime).First())
            .ToList();

        var users = await _revisionReader.ReadUsersAsync(unique.SelectMany(table => table.Revisions).Select(r => r.CreatedByUserId), cancellationToken);
        var versionIds = unique.SelectMany(table => table.Revisions).Select(r => r.SheetVersionId).OfType<int>().Distinct().ToList();
        var versions = await _dbContext.SheetVersions.AsNoTracking().Where(version => versionIds.Contains(version.Id)).ToListAsync(cancellationToken);

        var mapper = new SheetMapper(users.Values, versions);
        return [.. unique.Select(mapper.MapTableHeader).OrderBy(table => table.Title).ThenBy(table => table.Id)];
    }

    /// <summary>
    /// The sheet's submissions, oldest first; only those that published the version when one is given. Null when the
    /// sheet has no such version.
    /// </summary>
    public async Task<List<SheetSubmission>?> ReadSubmissionsAsync(int sheetId, int? versionNumber, CancellationToken cancellationToken)
    {
        var versions = await ReadVersionRowsAsync(sheetId, cancellationToken);
        var version = versionNumber is null ? null : versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
        if (versionNumber is not null && version is null)
        {
            return null;
        }

        var versionId = version?.Id;
        var submissions = await _dbContext.SheetSubmissions
            .Where(submission => submission.SheetId == sheetId && (versionId == null || submission.SheetVersionId == versionId))
            .Include(submission => submission.SubmittedByUser)
            .Include(submission => submission.Approvers)
                .ThenInclude(approver => approver.User)
            .OrderBy(submission => submission.SubmittedDateTime)
            .ThenBy(submission => submission.Id)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var mapper = new SheetMapper([], versions);
        return [.. submissions.Select(mapper.MapSubmission)];
    }

    /// <summary>
    /// The id of the sheet for the phase and sheet type, or null when there is none.
    /// </summary>
    public async Task<int?> FindSheetIdAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.SpecSheetPhaseId == phaseId && s.SpecSheetTypeId == sheetTypeId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>
    /// The sheets that match, without their tables, oldest first.
    /// </summary>
    public async Task<List<SheetSummary>> ReadSummariesAsync(Expression<Func<DbSheet, bool>> filter, CancellationToken cancellationToken)
    {
        var sheets = await _dbContext.Sheets
            .Where(filter)
            .Include(s => s.SpecSheetPhase)
            .Include(s => s.SpecSheetType)
            .Include(s => s.Versions)
            .Include(s => s.Properties)
                .ThenInclude(p => p.Revisions.Where(r => r.Status == RevisionStatus.Published
                    && r.SupersededDateTime == null))
            .Include(s => s.Properties)
                .ThenInclude(p => p.SpecSheetTypeProperty)
            .OrderBy(s => s.Id)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var typeIds = sheets.Select(s => s.SpecSheetTypeId).Distinct().ToList();
        var phaseIds = sheets.Select(s => s.SpecSheetPhaseId).Distinct().ToList();
        var drafts = await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(s => !s.IsPublished && typeIds.Contains(s.SpecSheetTypeId) && phaseIds.Contains(s.SpecSheetPhaseId))
            .Select(s => new { s.Id, s.SpecSheetTypeId, s.SpecSheetPhaseId })
            .ToListAsync(cancellationToken);
        var draftIds = drafts
            .GroupBy(draft => (draft.SpecSheetPhaseId, draft.SpecSheetTypeId))
            .ToDictionary(group => group.Key, group => group.Min(draft => draft.Id));

        var users = await _revisionReader.ReadUsersAsync(SheetMapper.SummaryUserIds(sheets), cancellationToken);
        var mapper = new SheetMapper(users.Values, sheets.SelectMany(sheet => sheet.Versions));

        return [.. sheets.Select(sheet => mapper.MapSummary(sheet, draftIds.TryGetValue((sheet.SpecSheetPhaseId, sheet.SpecSheetTypeId), out var draftId) ? draftId : null))];
    }

    /// <summary>
    /// Every published version of the sheets that match, without their tables: oldest sheet first, then oldest version
    /// first.
    /// </summary>
    public async Task<List<SheetVersionSummary>> ReadVersionSummariesAsync(Expression<Func<DbSheet, bool>> filter, CancellationToken cancellationToken)
    {
        var sheets = await _dbContext.Sheets
            .Where(filter)
            .Include(s => s.SpecSheetPhase)
            .Include(s => s.SpecSheetType)
            .Include(s => s.Versions)
            .Include(s => s.Properties)
                .ThenInclude(p => p.Revisions.Where(r => r.Status == RevisionStatus.Published))
            .Include(s => s.Properties)
                .ThenInclude(p => p.SpecSheetTypeProperty)
            .OrderBy(s => s.Id)
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var mapper = new SheetMapper([], sheets.SelectMany(sheet => sheet.Versions));
        return [.. sheets.SelectMany(mapper.MapVersionSummaries)];
    }

    /// <summary>
    /// Where the spec sheet with this SpecSheets id is among the sheets, or null when it isn't: it hasn't been migrated to
    /// a version, or it is an unpublished one with no sheet.
    /// </summary>
    public async Task<SheetLocation?> FindLocationAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var version = await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(v => v.SourceSpecSheetId == specSheetId)
            .OrderByDescending(v => v.Id)
            .Select(v => new { v.SheetId, v.VersionNumber })
            .FirstOrDefaultAsync(cancellationToken);
        if (version is not null)
        {
            return new SheetLocation { SpecSheetId = specSheetId, SheetId = version.SheetId, VersionNumber = version.VersionNumber, IsDraft = false };
        }

        var draftSheetId = await FindDraftSheetIdAsync(specSheetId, cancellationToken);
        return draftSheetId is null
            ? null
            : new SheetLocation { SpecSheetId = specSheetId, SheetId = draftSheetId.Value, VersionNumber = null, IsDraft = true };
    }

    // The sheet that holds the drafts of the unpublished spec sheet with this id, or null when the spec sheet is published.
    private async Task<int?> FindDraftSheetIdAsync(int specSheetId, CancellationToken cancellationToken)
    {
        var specSheet = await _dbContext.SpecSheets
            .AsNoTracking()
            .Where(s => s.Id == specSheetId)
            .Select(s => new { s.IsPublished, s.SpecSheetTypeId, s.SpecSheetPhaseId })
            .FirstOrDefaultAsync(cancellationToken);
        if (specSheet is null || specSheet.IsPublished)
        {
            return null;
        }

        return await _dbContext.Sheets
            .AsNoTracking()
            .Where(s => s.SpecSheetTypeId == specSheet.SpecSheetTypeId && s.SpecSheetPhaseId == specSheet.SpecSheetPhaseId)
            .Select(s => (int?)s.Id)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new InvalidOperationException($"Spec Sheet with Id {specSheetId} has no sheet; it hasn't been migrated.");
    }

    private static SheetCells MapCells(Sheet sheet, IReadOnlyCollection<Guid> referenceIds)
    {
        var requested = referenceIds.ToHashSet();
        return new SheetCells
        {
            Version = sheet.Version!,
            SheetId = sheet.Id,
            SpecSheetTypeId = sheet.SpecSheetTypeId,
            SpecSheetPhaseId = sheet.SpecSheetPhaseId,
            Properties = sheet.Properties,
            Cells = [.. sheet.Tables
                .SelectMany(table => table.Groups)
                .SelectMany(group => group.Rows)
                .SelectMany(row => row.Cells)
                .Where(cell => requested.Contains(cell.ReferenceId))]
        };
    }

    private async Task<Sheet?> ReadPublishedAsync(int sheetId, Func<List<DbSheetVersion>, DbSheetVersion?> chooseVersion, CancellationToken cancellationToken)
    {
        var versions = await ReadVersionRowsAsync(sheetId, cancellationToken);
        var version = chooseVersion(versions);
        if (version is null)
        {
            return null;
        }

        return await ReadAsync(sheetId, version, versions, version.PublishedDateTime, false, cancellationToken);
    }

    /// <summary>
    /// The sheet as it stands now with its drafts, each item's draft where it has one. Null when there is no such sheet.
    /// </summary>
    public async Task<Sheet?> ReadDraftAsync(int sheetId, CancellationToken cancellationToken)
    {
        if (!await _dbContext.Sheets.AnyAsync(s => s.Id == sheetId, cancellationToken))
        {
            return null;
        }

        var versions = await ReadVersionRowsAsync(sheetId, cancellationToken);
        var latest = versions.LastOrDefault();

        // A version published within a second of the one before it is given the next whole second, which can be just
        // ahead of now; the sheet as it stands includes it all the same.
        var moment = latest is not null && latest.PublishedDateTime > DateTime.UtcNow ? latest.PublishedDateTime : DateTime.UtcNow;
        return await ReadAsync(sheetId, latest, versions, moment, true, cancellationToken);
    }

    private async Task<Sheet> ReadAsync(int sheetId, DbSheetVersion? version, List<DbSheetVersion> versions, DateTime moment, bool withDrafts, CancellationToken cancellationToken)
    {
        var sheet = await _revisionReader.ReadSheetAsync(sheetId, moment, withDrafts, cancellationToken);
        var linkedTables = await ReadLinkedTablesAsync(sheetId, moment, withDrafts, cancellationToken);
        var users = await _revisionReader.ReadUsersAsync(SheetMapper.UserIds(sheet, versions, linkedTables), cancellationToken);

        return new SheetMapper(users.Values, versions).Map(sheet, version, linkedTables, moment, withDrafts);
    }

    private async Task<List<DbSheetVersion>> ReadVersionRowsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _dbContext.SheetVersions
            .AsNoTracking()
            .Where(version => version.SheetId == sheetId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    // The linked tables shown at the moment, read like the sheet's other items: those whose revision then doesn't
    // remove them, with that revision alone.
    private async Task<List<DbSheetLinkedTable>> ReadLinkedTablesAsync(int sheetId, DateTime moment, bool withDrafts, CancellationToken cancellationToken)
    {
        return await _dbContext.SheetLinkedTables
            .Where(linked => linked.SheetId == sheetId
                && linked.Revisions.Any(r => !r.IsDeleted
                    && ((withDrafts && r.Status == RevisionStatus.Draft)
                        || (r.Status == RevisionStatus.Published
                            && r.PublishedDateTime <= moment
                            && (r.SupersededDateTime == null || r.SupersededDateTime > moment)))))
            .Include(linked => linked.Revisions.Where(r => (withDrafts && r.Status == RevisionStatus.Draft)
                || (r.Status == RevisionStatus.Published
                    && r.PublishedDateTime <= moment
                    && (r.SupersededDateTime == null || r.SupersededDateTime > moment))))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
