using System.Globalization;

namespace PUSpecSheet.Application.Published;

/// <summary>
/// Works out which row of a linked column a picked value came from, from what the queries returned.
/// </summary>
public static class PublishedLinkedRows
{
    /// <summary>A published value as the text a linked dropdown stores when it is picked.</summary>
    public static string OptionText(object value)
    {
        return value switch
        {
            string text => text.Trim(),
            DateOnly date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => Convert.ToString(value, CultureInfo.InvariantCulture) ?? string.Empty,
        };
    }

    /// <summary>
    /// The row each value of a column belongs to, by the value as text. Cells in a removed section or
    /// column block are left out, and when several rows hold the same value it is the first, top to bottom.
    /// </summary>
    /// <param name="structure">What was on the sheet at the version.</param>
    /// <param name="column">The cells of one column of one table.</param>
    /// <param name="values">Their values, by cell id.</param>
    public static Dictionary<string, Guid> RowByOption(
        PublishedStructure structure,
        IReadOnlyList<PublishedLookupCellRecord> column,
        IReadOnlyDictionary<int, object> values)
    {
        var sections = structure.Sections.ToDictionary(section => section.Id);
        var sectionOrder = PublishedLookupAssembler.SectionOrder(sections.Values);
        var shown = column
            .Where(cell => sections.ContainsKey(cell.SectionId)
                && (cell.ColumnBlockId is not { } blockId || structure.ColumnBlock(blockId) is not null)
                && values.ContainsKey(cell.CellId))
            .OrderBy(cell => sectionOrder[cell.SectionId])
            .ThenBy(cell => cell.RowOrder)
            .ThenBy(cell => cell.RowId)
            .ThenBy(cell => cell.Column)
            .ThenBy(cell => cell.CellId);

        var rows = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var cell in shown)
        {
            rows.TryAdd(OptionText(values[cell.CellId]), cell.RowPublicId);
        }

        return rows;
    }

    /// <summary>
    /// The linked rows of some cells of one row, by the value picked in each, or null when there are none.
    /// </summary>
    public static Dictionary<string, Guid>? Of(
        IEnumerable<PublishedLookupCellRecord> cells,
        IReadOnlyDictionary<int, object> values,
        IReadOnlyDictionary<int, Guid>? linkedRows)
    {
        if (linkedRows is null || linkedRows.Count == 0)
        {
            return null;
        }

        var result = new Dictionary<string, Guid>(StringComparer.Ordinal);
        foreach (var cell in cells)
        {
            if (linkedRows.TryGetValue(cell.CellId, out var rowId) && values.TryGetValue(cell.CellId, out var value))
            {
                result.TryAdd(OptionText(value), rowId);
            }
        }

        return result.Count == 0 ? null : result;
    }
}
