using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Types;
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
        var versions = await ReadVersionRowsAsync(sheetId, cancellationToken);
        var version = versionNumber is null
            ? versions.LastOrDefault()
            : versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
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
            .AsSplitQuery()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }
}
