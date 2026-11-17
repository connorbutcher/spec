namespace PUSpecSheet.DataAccess.Sheets.Layout;

public static class SheetCellPlanner
{
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
