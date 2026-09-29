using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Works out the cells a new row starts with.</summary>
internal static class NewRowCells
{
    /// <summary>
    /// Copies the columns of <paramref name="source"/> (type, column and column span, single row span),
    /// skipping any column that a cell in an earlier row still spans down into at
    /// <paramref name="insertIndex"/> (0-based). A section without rows starts with one cell of
    /// <paramref name="defaultCellTypeId"/>.
    /// </summary>
    public static List<TemplateCell> For(
        IReadOnlyList<TemplateRow> orderedRows,
        TemplateRow? source,
        int insertIndex,
        int defaultCellTypeId)
    {
        if (source is null)
        {
            return [new TemplateCell { Column = 1, CellTypeId = defaultCellTypeId }];
        }

        var covered = CoveredColumns(orderedRows, insertIndex);

        return source.Cells
            .Where(cell => !Enumerable.Range(cell.Column, cell.ColumnSpan).Any(covered.Contains))
            .Select(cell => new TemplateCell
            {
                Column = cell.Column,
                ColumnSpan = cell.ColumnSpan,
                CellTypeId = cell.CellTypeId,
            })
            .ToList();
    }

    /// <summary>The columns that cells in rows before <paramref name="rowIndex"/> (0-based) span down into it.</summary>
    private static HashSet<int> CoveredColumns(IReadOnlyList<TemplateRow> orderedRows, int rowIndex)
    {
        var covered = new HashSet<int>();
        for (var index = 0; index < Math.Min(rowIndex, orderedRows.Count); index++)
        {
            foreach (var cell in orderedRows[index].Cells.Where(cell => index + cell.RowSpan > rowIndex))
            {
                covered.UnionWith(Enumerable.Range(cell.Column, cell.ColumnSpan));
            }
        }

        return covered;
    }
}
