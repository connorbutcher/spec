using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Works out the cells a row added to the end of a section starts with.</summary>
internal static class NewRowCells
{
    /// <summary>
    /// Copies the columns of the section's last row (type, column and column span, single row span),
    /// skipping any column still covered by a row span from a row above. A section without rows
    /// starts with one cell of <paramref name="defaultCellTypeId"/>.
    /// </summary>
    public static List<TemplateCell> For(IReadOnlyList<TemplateRow> orderedRows, int defaultCellTypeId)
    {
        if (orderedRows.Count == 0)
        {
            return [new TemplateCell { Column = 1, CellTypeId = defaultCellTypeId }];
        }

        var newRowIndex = orderedRows.Count;
        var covered = CoveredColumns(orderedRows, newRowIndex);

        return orderedRows[^1].Cells
            .Where(cell => !Enumerable.Range(cell.Column, cell.ColumnSpan).Any(covered.Contains))
            .Select(cell => new TemplateCell
            {
                Column = cell.Column,
                ColumnSpan = cell.ColumnSpan,
                CellTypeId = cell.CellTypeId,
            })
            .ToList();
    }

    /// <summary>The columns that cells in earlier rows span down into the row at <paramref name="rowIndex"/> (0-based).</summary>
    private static HashSet<int> CoveredColumns(IReadOnlyList<TemplateRow> orderedRows, int rowIndex)
    {
        var covered = new HashSet<int>();
        for (var index = 0; index < orderedRows.Count; index++)
        {
            foreach (var cell in orderedRows[index].Cells.Where(cell => index + cell.RowSpan > rowIndex))
            {
                covered.UnionWith(Enumerable.Range(cell.Column, cell.ColumnSpan));
            }
        }

        return covered;
    }
}
