using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Application.Templates;

/// <summary>Works out the cells a new row starts with.</summary>
internal static class NewRowCells
{
    /// <summary>
    /// Copies the columns of <paramref name="source"/> (type, column block, column and column span, single
    /// row span), skipping any column that a cell in an earlier row still spans down into at
    /// <paramref name="insertIndex"/> (0-based). A section without rows starts with one cell of
    /// <paramref name="defaultCellTypeId"/>, plus one in each of <paramref name="columnBlockIds"/>.
    /// </summary>
    public static List<TemplateCell> For(
        IReadOnlyList<TemplateRow> orderedRows,
        TemplateRow? source,
        int insertIndex,
        int defaultCellTypeId,
        IReadOnlyList<int> columnBlockIds)
    {
        if (source is null)
        {
            return columnBlockIds
                .Select(blockId => (int?)blockId)
                .Prepend(null)
                .Select(blockId => new TemplateCell
                {
                    TemplateColumnBlockId = blockId,
                    Column = 1,
                    CellTypeId = defaultCellTypeId,
                })
                .ToList();
        }

        var covered = CoveredColumns(orderedRows, insertIndex);

        return source.Cells
            .Where(cell => !Enumerable.Range(cell.Column, cell.ColumnSpan)
                .Any(column => covered.Contains((cell.TemplateColumnBlockId, column))))
            .Select(cell => new TemplateCell
            {
                TemplateColumnBlockId = cell.TemplateColumnBlockId,
                Column = cell.Column,
                ColumnSpan = cell.ColumnSpan,
                CellTypeId = cell.CellTypeId,
            })
            .ToList();
    }

    /// <summary>
    /// The columns, by column block (null for the row's own cells), that cells in rows before
    /// <paramref name="rowIndex"/> (0-based) span down into it.
    /// </summary>
    private static HashSet<(int? BlockId, int Column)> CoveredColumns(IReadOnlyList<TemplateRow> orderedRows, int rowIndex)
    {
        var covered = new HashSet<(int? BlockId, int Column)>();
        for (var index = 0; index < Math.Min(rowIndex, orderedRows.Count); index++)
        {
            foreach (var cell in orderedRows[index].Cells.Where(cell => index + cell.RowSpan > rowIndex))
            {
                covered.UnionWith(Enumerable.Range(cell.Column, cell.ColumnSpan)
                    .Select(column => (cell.TemplateColumnBlockId, column)));
            }
        }

        return covered;
    }
}
