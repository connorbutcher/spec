using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>What counts as "the same" when deciding whether a draft still differs from what is published.</summary>
public static class SheetRevisionComparer
{
    /// <summary>The same place in the order and the same existence (neither removed nor restored).</summary>
    public static bool SameStructure(ISheetRevision draft, ISheetRevision current)
    {
        return draft.DisplayOrder == current.DisplayOrder && draft.IsDeleted == current.IsDeleted;
    }

    /// <summary>Text that differs only by surrounding spaces, or by being empty rather than missing, is the same.</summary>
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
