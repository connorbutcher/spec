using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Published.Queries;

/// <summary>Sheets and their versions as other applications find them: by phase code and public identifier.</summary>
public static class PublishedSheetQueries
{
    /// <summary>
    /// The phase's sheets of the sheet type that match every filter, each with its latest version and
    /// identifier values by key, oldest first.
    /// </summary>
    public static async Task<List<PublishedSheetReferenceDto>> ListReferencesAsync(
        PuSpecSheetDbContext db,
        string phaseCode,
        int sheetTypeId,
        IReadOnlyList<PublishedIdentifierFilter> filters,
        CancellationToken cancellationToken)
    {
        var sheets = db.Sheets
            .AsNoTracking()
            .Where(sheet => sheet.Phase.Code == phaseCode && sheet.SheetTypeId == sheetTypeId);
        foreach (var filter in filters)
        {
            var identifierId = filter.IdentifierId;
            var value = filter.Value;
            sheets = value is null
                ? sheets.Where(sheet => !sheet.IdentifierValues.Any(candidate => candidate.SheetTypeIdentifierId == identifierId))
                : sheets.Where(sheet => sheet.IdentifierValues.Any(candidate => candidate.SheetTypeIdentifierId == identifierId && candidate.Value == value));
        }

        var found = await sheets
            .OrderBy(sheet => sheet.Id)
            .Select(sheet => new
            {
                sheet.PublicId,
                sheet.Phase.Code,
                sheet.SheetTypeId,
                LatestVersion = sheet.Versions.Max(version => (int?)version.VersionNumber),
                Identifiers = sheet.IdentifierValues.Select(value => new { value.Identifier.Key, value.Value }).ToList(),
            })
            .ToListAsync(cancellationToken);

        return found
            .Select(sheet => new PublishedSheetReferenceDto(sheet.PublicId, sheet.Code, sheet.SheetTypeId, sheet.LatestVersion)
            {
                Identifiers = sheet.Identifiers.ToDictionary(value => value.Key, value => value.Value),
            })
            .ToList();
    }

    public static async Task<bool> ExistsAsync(PuSpecSheetDbContext db, Guid sheetPublicId, CancellationToken cancellationToken)
    {
        return await db.Sheets.AnyAsync(sheet => sheet.PublicId == sheetPublicId, cancellationToken);
    }

    /// <summary>The sheet's versions, oldest first.</summary>
    public static async Task<List<PublishedVersionDto>> ListVersionsAsync(PuSpecSheetDbContext db, Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var versions = await db.SheetVersions
            .AsNoTracking()
            .Where(version => version.Sheet.PublicId == sheetPublicId)
            .OrderBy(version => version.VersionNumber)
            .Select(version => new { version.VersionNumber, version.PublishedAtUtc, version.Note })
            .ToListAsync(cancellationToken);

        return versions
            .Select(version => new PublishedVersionDto(
                version.VersionNumber,
                DateTime.SpecifyKind(version.PublishedAtUtc, DateTimeKind.Utc),
                version.Note))
            .ToList();
    }

    /// <summary>
    /// The version a point names: the version with its number, else the last one published at or before its
    /// moment, else the latest; null when there is none.
    /// </summary>
    public static async Task<PublishedVersionStamp?> FindVersionAsync(PuSpecSheetDbContext db, Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return await QueryVersions(db, sheetPublicId, point).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>The versions a point could name, newest first: the first is the one it names.</summary>
    public static IQueryable<PublishedVersionStamp> QueryVersions(PuSpecSheetDbContext db, Guid sheetPublicId, PublishedVersionPoint point)
    {
        var versions = db.SheetVersions
            .AsNoTracking()
            .Where(version => version.Sheet.PublicId == sheetPublicId);

        if (point.VersionNumber is { } number)
        {
            versions = versions.Where(version => version.VersionNumber == number);
        }
        else if (point.AtUtc is { } moment)
        {
            versions = versions.Where(version => version.PublishedAtUtc <= moment);
        }

        return versions
            .OrderByDescending(version => version.VersionNumber)
            .Select(version => new PublishedVersionStamp(version.SheetId, version.VersionNumber, version.PublishedAtUtc));
    }

    /// <summary>Every version of the sheets published at or before the moment.</summary>
    public static async Task<List<PublishedVersionStamp>> ListVersionsUpToAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> sheetIds, DateTime moment, CancellationToken cancellationToken)
    {
        return await db.SheetVersions
            .AsNoTracking()
            .Where(version => sheetIds.Contains(version.SheetId) && version.PublishedAtUtc <= moment)
            .Select(version => new PublishedVersionStamp(version.SheetId, version.VersionNumber, version.PublishedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
