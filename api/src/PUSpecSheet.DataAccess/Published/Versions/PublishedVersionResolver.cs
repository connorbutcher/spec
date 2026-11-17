using PUSpecSheet.DataAccess.Common;

namespace PUSpecSheet.DataAccess.Published.Versions;

// A sheet only changes when published, so "as at a moment" is the newest version published at or before it.
public sealed class PublishedVersionResolver
{
    private readonly IPublishedSheetDataService _publishedSheetData;

    public PublishedVersionResolver(IPublishedSheetDataService publishedSheetData)
    {
        _publishedSheetData = publishedSheetData;
    }

    public async Task<ResolvedSheetVersion> ResolveAsync(
        Guid sheetPublicId,
        PublishedVersionPoint point,
        CancellationToken cancellationToken)
    {
        var found = await _publishedSheetData.FindVersionAsync(sheetPublicId, point, cancellationToken)
            ?? throw new NotFoundException(Describe(sheetPublicId, point));

        return new ResolvedSheetVersion
        {
            SheetId = found.SheetId,
            SheetPublicId = sheetPublicId,
            VersionNumber = found.VersionNumber,
            PublishedAtUtc = DateTime.SpecifyKind(found.PublishedAtUtc, DateTimeKind.Utc),
        };
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
