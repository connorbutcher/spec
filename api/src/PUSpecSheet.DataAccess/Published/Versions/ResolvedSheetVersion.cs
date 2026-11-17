using PUSpecSheet.DataAccess.Published.Rows;
using PUSpecSheet.DataAccess.Published.Sheets;

namespace PUSpecSheet.DataAccess.Published.Versions;

public sealed record ResolvedSheetVersion
{
    public required int SheetId { get; init; }

    public required Guid SheetPublicId { get; init; }

    public required int VersionNumber { get; init; }

    public required DateTime PublishedAtUtc { get; init; }

    public string KeyFor(PublishedSheetSelection selection)
    {
        return $"{SheetPublicId:N}-v{VersionNumber}-{selection.Key}";
    }

    public string KeyFor(PublishedRowsSelection selection)
    {
        return $"{SheetPublicId:N}-v{VersionNumber}-{selection.Key}";
    }
}
