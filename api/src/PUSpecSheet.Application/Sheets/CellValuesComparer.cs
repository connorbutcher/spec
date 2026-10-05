namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Compares the cell values of two row revisions the way a person would: a cell with no value, an empty text
/// and an unticked box are all "nothing", text ignores surrounding spaces, and numbers are equal when their
/// values are, however they were written.
/// </summary>
public static class CellValuesComparer
{
    /// <summary>Whether two sets of values, each by sheet cell id, hold the same thing in every cell.</summary>
    public static bool Same(
        IReadOnlyDictionary<int, CellValueBag>? left,
        IReadOnlyDictionary<int, CellValueBag>? right)
    {
        left ??= new Dictionary<int, CellValueBag>();
        right ??= new Dictionary<int, CellValueBag>();

        foreach (var cellId in left.Keys.Union(right.Keys))
        {
            if (!SameValue(left.GetValueOrDefault(cellId), right.GetValueOrDefault(cellId)))
            {
                return false;
            }
        }

        return true;
    }

    private static bool SameValue(CellValueBag? left, CellValueBag? right)
    {
        return SheetRevisionComparer.SameText(left?.Text, right?.Text)
            && left?.Number == right?.Number
            && left?.Date == right?.Date
            && (left?.Boolean ?? false) == (right?.Boolean ?? false)
            && left?.OptionId == right?.OptionId;
    }
}
