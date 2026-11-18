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
/// mapped by <see cref="SheetMapper"/>. Nothing is read from the copies, and nothing is numbered: every id is the id of
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
            .Include(s => s.Properties.Where(p => p.Revisions.Any(r => !r.IsDeleted
                    && r.Status == RevisionStatus.Published
                    && r.SupersededDateTime == null)))
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
    /// Where the spec sheet with this SpecSheets id is among the sheets, or null when it isn't: it hasn't been migrated to
    /// a version, or it is an unpublished one whose type isn't edited on the revisions.
    /// </summary>
    public async Task<SheetLocation?> FindLocationAsync(int specSheetId, SheetEditSwitch editSwitch, CancellationToken cancellationToken)
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

        var draftSheetId = await editSwitch.FindDraftSheetIdAsync(specSheetId, cancellationToken);
        return draftSheetId is null
            ? null
            : new SheetLocation { SpecSheetId = specSheetId, SheetId = draftSheetId.Value, VersionNumber = null, IsDraft = true };
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
        return await ReadAsync(sheetId, versions.LastOrDefault(), versions, DateTime.UtcNow, true, cancellationToken);
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
