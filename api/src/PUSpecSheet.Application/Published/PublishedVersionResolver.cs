using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence.Published;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Finds the published version a caller means, in one query. A sheet only changes when it is published,
/// so "as it stood at a moment" is the newest version published at or before that moment.
/// </summary>
public sealed class PublishedVersionResolver(IPublishedSheetRepository sheets)
{
    public async Task<ResolvedSheetVersion> ResolveAsync(
        Guid sheetPublicId,
        PublishedVersionPoint point,
        CancellationToken cancellationToken)
    {
        var found = await sheets.FindVersionAsync(sheetPublicId, point, cancellationToken)
            ?? throw new NotFoundException(Describe(sheetPublicId, point));

        return new ResolvedSheetVersion(
            found.SheetId,
            sheetPublicId,
            found.VersionNumber,
            DateTime.SpecifyKind(found.PublishedAtUtc, DateTimeKind.Utc));
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
