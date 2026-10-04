using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Finds the published version a caller means, in one query. A sheet only changes when it is published,
/// so "as it stood at a moment" is the newest version published at or before that moment.
/// </summary>
public sealed class PublishedVersionResolver(PuSpecSheetDbContext db)
{
    public async Task<ResolvedSheetVersion> ResolveAsync(
        Guid sheetPublicId,
        PublishedVersionPoint point,
        CancellationToken cancellationToken)
    {
        var found = await Query(sheetPublicId, point)
            .Select(version => new { version.SheetId, version.VersionNumber, version.PublishedAtUtc })
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException(Describe(sheetPublicId, point));

        return new ResolvedSheetVersion(
            found.SheetId,
            sheetPublicId,
            found.VersionNumber,
            DateTime.SpecifyKind(found.PublishedAtUtc, DateTimeKind.Utc));
    }

    /// <summary>The versions the point allows, newest first; the first is the one the caller means.</summary>
    public IQueryable<SheetVersion> Query(Guid sheetPublicId, PublishedVersionPoint point)
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

        return versions.OrderByDescending(version => version.VersionNumber);
    }

    private static string Describe(Guid sheetPublicId, PublishedVersionPoint point)
    {
        if (point.VersionNumber is { } number)
        {
            return $"Sheet {sheetPublicId} has no version {number}.";
        }

        return point.AtUtc is { } moment
            ? $"Sheet {sheetPublicId} had no published version at {moment:u}."
            : $"Sheet {sheetPublicId} has no published version.";
    }
}
