namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Works out which cells a horizontal table's rows are missing. The rule it enforces is that every row
/// holds a cell for each template cell of its template row: the row's own cells once, and each column
/// block's cells once for every copy of that block on the table. So adding a row fills it in across all
/// the block copies, and adding a block copy gives every existing row its cells in it.
/// </summary>
public static class SheetCellPlanner
{
    /// <summary>
    /// The (row, template cell, column block copy) combinations that should exist but aren't in
    /// <paramref name="existing"/>. A null block copy is one of the row's own cells. Running it again after
    /// the result has been added returns nothing.
    /// </summary>
    public static IReadOnlyList<(int SheetRowId, int TemplateCellId, int? SheetColumnBlockId)> Missing(
        IEnumerable<(int SheetRowId, int TemplateRowId)> rows,
        IEnumerable<(int TemplateCellId, int TemplateRowId, int? TemplateColumnBlockId)> templateCells,
        IEnumerable<(int SheetColumnBlockId, int TemplateColumnBlockId)> blocks,
        IEnumerable<(int SheetRowId, int TemplateCellId, int? SheetColumnBlockId)> existing)
    {
        var cellsByRow = templateCells.ToLookup(cell => cell.TemplateRowId);
        var copiesByTemplate = blocks.ToLookup(block => block.TemplateColumnBlockId, block => block.SheetColumnBlockId);
        var have = existing.ToHashSet();

        var missing = new List<(int, int, int?)>();
        foreach (var (rowId, templateRowId) in rows)
        {
            foreach (var cell in cellsByRow[templateRowId])
            {
                if (cell.TemplateColumnBlockId is not { } templateBlockId)
                {
                    AddIfMissing(missing, have, rowId, cell.TemplateCellId, null);
                    continue;
                }

                foreach (var copyId in copiesByTemplate[templateBlockId])
                {
                    AddIfMissing(missing, have, rowId, cell.TemplateCellId, copyId);
                }
            }
        }

        return missing;
    }

    private static void AddIfMissing(
        List<(int, int, int?)> missing,
        HashSet<(int, int, int?)> have,
        int rowId,
        int templateCellId,
        int? copyId)
    {
        if (!have.Contains((rowId, templateCellId, copyId)))
        {
            missing.Add((rowId, templateCellId, copyId));
        }
    }
}
