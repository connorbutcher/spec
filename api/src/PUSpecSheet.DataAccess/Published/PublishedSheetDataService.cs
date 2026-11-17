using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Phases;
using PUSpecSheet.Abstractions.Published;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.DataAccess.Published.Queries;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Published;

public sealed class PublishedSheetDataService : IPublishedSheetDataService
{
    private readonly PuSpecSheetDbContext _db;

    public PublishedSheetDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<PublishedSheetReference>> ListReferencesAsync(
        string phaseCode,
        int sheetTypeId,
        IReadOnlyList<PublishedIdentifierFilter> filters,
        CancellationToken cancellationToken)
    {
        var sheets = _db.Sheets
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
            .Select(sheet => new PublishedSheetReference
            {
                Sheet = sheet.PublicId,
                Phase = sheet.Code,
                SheetType = sheet.SheetTypeId,
                LatestVersion = sheet.LatestVersion,
                Identifiers = sheet.Identifiers.ToDictionary(value => value.Key, value => value.Value),
            })
            .ToList();
    }

    public async Task<bool> ExistsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        return await _db.Sheets.AnyAsync(sheet => sheet.PublicId == sheetPublicId, cancellationToken);
    }

    public async Task<List<PublishedVersion>> ListVersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var versions = await _db.SheetVersions
            .AsNoTracking()
            .Where(version => version.Sheet.PublicId == sheetPublicId)
            .OrderBy(version => version.VersionNumber)
            .Select(version => new { version.VersionNumber, version.PublishedAtUtc, version.Note })
            .ToListAsync(cancellationToken);

        return versions
            .Select(version => new PublishedVersion
            {
                Version = version.VersionNumber,
                PublishedAtUtc = DateTime.SpecifyKind(version.PublishedAtUtc, DateTimeKind.Utc),
                Note = version.Note,
            })
            .ToList();
    }

    public async Task<PublishedVersionStamp?> FindVersionAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return await QueryVersions(sheetPublicId, point).FirstOrDefaultAsync(cancellationToken);
    }

    public IQueryable<PublishedVersionStamp> QueryVersions(Guid sheetPublicId, PublishedVersionPoint point)
    {
        var versions = _db.SheetVersions
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
            .Select(version => new PublishedVersionStamp
            {
                SheetId = version.SheetId,
                VersionNumber = version.VersionNumber,
                PublishedAtUtc = version.PublishedAtUtc,
            });
    }

    public async Task<List<PublishedVersionStamp>> ListVersionsUpToAsync(IReadOnlyCollection<int> sheetIds, DateTime moment, CancellationToken cancellationToken)
    {
        return await _db.SheetVersions
            .AsNoTracking()
            .Where(version => sheetIds.Contains(version.SheetId) && version.PublishedAtUtc <= moment)
            .Select(version => new PublishedVersionStamp
            {
                SheetId = version.SheetId,
                VersionNumber = version.VersionNumber,
                PublishedAtUtc = version.PublishedAtUtc,
            })
            .ToListAsync(cancellationToken);
    }
}
