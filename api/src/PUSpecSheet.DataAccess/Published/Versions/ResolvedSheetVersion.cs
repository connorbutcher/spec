using PUSpecSheet.DataAccess.Published.Rows;
using PUSpecSheet.DataAccess.Published.Sheets;

namespace PUSpecSheet.DataAccess.Published.Versions;

/// <summary>
/// One published version of one sheet, found from a <see cref="PublishedVersionPoint"/>. Everything read
/// for a caller is keyed on this, because a published version never changes.
/// </summary>
public sealed record ResolvedSheetVersion
{
    public ResolvedSheetVersion(int sheetId, Guid sheetPublicId, int versionNumber, DateTime publishedAtUtc)
    {
        SheetId = sheetId;
        SheetPublicId = sheetPublicId;
        VersionNumber = versionNumber;
        PublishedAtUtc = publishedAtUtc;
    }

    public int SheetId { get; init; }

    public Guid SheetPublicId { get; init; }

    public int VersionNumber { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    /// <summary>
    /// Names one answer: this version read with one selection. It is the cache key and, quoted, the ETag.
    /// </summary>
    public string KeyFor(PublishedSheetSelection selection)
    {
        return $"{SheetPublicId:N}-v{VersionNumber}-{selection.Key}";
    }

    /// <summary>Names one answer: this version's rows read with one selection. It is the cache key and, quoted, the ETag.</summary>
    public string KeyFor(PublishedRowsSelection selection)
    {
        return $"{SheetPublicId:N}-v{VersionNumber}-{selection.Key}";
    }
}
