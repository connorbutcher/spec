using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.DataAccess.Templates.Rows;

internal static class NewRowCells
{
    // Columns that a cell in an earlier row still spans down into are skipped, so the new row never overlaps them.
    public static List<DbTemplateCell> For(
        IReadOnlyList<DbTemplateRow> orderedRows,
        DbTemplateRow? source,
        int insertIndex,
        int defaultCellTypeId,
        IReadOnlyList<int> columnBlockIds)
    {
        if (source is null)
        {
            return columnBlockIds
                .Select(blockId => (int?)blockId)
                .Prepend(null)
                .Select(blockId => new DbTemplateCell
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
            .Select(cell => new DbTemplateCell
            {
                TemplateColumnBlockId = cell.TemplateColumnBlockId,
                Column = cell.Column,
                ColumnSpan = cell.ColumnSpan,
                CellTypeId = cell.CellTypeId,
            })
            .ToList();
    }

    private static HashSet<(int? BlockId, int Column)> CoveredColumns(IReadOnlyList<DbTemplateRow> orderedRows, int rowIndex)
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
