namespace PUSpecSheet.Application.Published;

/// <summary>
/// One published version of one sheet, found from a <see cref="PublishedVersionPoint"/>. Everything read
/// for a caller is keyed on this, because a published version never changes.
/// </summary>
public sealed record ResolvedSheetVersion(int SheetId, Guid SheetPublicId, int VersionNumber, DateTime PublishedAtUtc)
{
    /// <summary>
    /// Names one answer: this version read with one selection. It is the cache key and, quoted, the ETag.
    /// </summary>
    public string KeyFor(PublishedSheetSelection selection)
    {
        return $"{SheetPublicId:N}-v{VersionNumber}-{selection.Key}";
    }
}
