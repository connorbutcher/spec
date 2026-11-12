using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Published;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Data.Repositories.Published;

public sealed class PublishedSheetRepository(PuSpecSheetDbContext db) : IPublishedSheetRepository
{
    public async Task<PublishedSheetReferenceDto?> FindReferenceAsync(string phaseCode, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .Where(sheet => sheet.Phase.Code == phaseCode && sheet.SheetTypeId == sheetTypeId)
            .Select(sheet => new PublishedSheetReferenceDto(
                sheet.PublicId,
                sheet.Phase.Code,
                sheet.SheetTypeId,
                sheet.Versions.Max(version => (int?)version.VersionNumber)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        return await db.Sheets.AnyAsync(sheet => sheet.PublicId == sheetPublicId, cancellationToken);
    }

    public async Task<List<PublishedVersionDto>> ListVersionsAsync(Guid sheetPublicId, CancellationToken cancellationToken)
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

    public async Task<PublishedVersionStamp?> FindVersionAsync(Guid sheetPublicId, PublishedVersionPoint point, CancellationToken cancellationToken)
    {
        return await QueryVersions(sheetPublicId, point).FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>The versions a point could name, newest first: the first is the one it names.</summary>
    public IQueryable<PublishedVersionStamp> QueryVersions(Guid sheetPublicId, PublishedVersionPoint point)
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

    public async Task<List<PublishedVersionStamp>> ListVersionsUpToAsync(IReadOnlyCollection<int> sheetIds, DateTime moment, CancellationToken cancellationToken)
    {
        return await db.SheetVersions
            .AsNoTracking()
            .Where(version => sheetIds.Contains(version.SheetId) && version.PublishedAtUtc <= moment)
            .Select(version => new PublishedVersionStamp(version.SheetId, version.VersionNumber, version.PublishedAtUtc))
            .ToListAsync(cancellationToken);
    }
}
