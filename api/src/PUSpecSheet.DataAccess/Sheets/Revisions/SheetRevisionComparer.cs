using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Revisions;

public static class SheetRevisionComparer
{
    public static bool SameStructure(IDbSheetRevision draft, IDbSheetRevision current)
    {
        return draft.DisplayOrder == current.DisplayOrder && draft.IsDeleted == current.IsDeleted;
    }

    // Text that differs only by surrounding spaces, or by being empty rather than missing, is the same.
    public static bool SameText(string? left, string? right)
    {
        return string.Equals(Normalise(left), Normalise(right), StringComparison.Ordinal);
    }

    private static string? Normalise(string? text)
    {
        var trimmed = text?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
