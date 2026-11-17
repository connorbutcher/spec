using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Reading;

// Takes some rows out of a live sheet, with what follows from them, so a change to rows can be answered without
// sending the whole sheet. The rows are cut from the sheet as it is built for the viewer, so they are exactly what a
// whole read would show.
public static class SheetRowsChanges
{
    public static SheetRowsChange From(Sheet sheet, IReadOnlyCollection<int> rowIds)
    {
        var wanted = rowIds.ToHashSet();
        var rows = new List<SheetRow>();
        var tableIds = new HashSet<int>();
        foreach (var table in sheet.Tables)
        {
            var before = rows.Count;
            Collect(table.Sections, wanted, rows);
            if (rows.Count > before)
            {
                tableIds.Add(table.Id);
            }
        }

        var templateCellIds = rows.SelectMany(row => row.Cells).Select(cell => cell.TemplateCellId).ToHashSet();
        return new SheetRowsChange
        {
            SheetId = sheet.Id,
            Rows = rows,
            LatestVersionNumber = sheet.LatestVersionNumber,
            MyDraftCount = sheet.MyDraftCount,
            OtherDrafts = sheet.OtherDrafts,
            LinkedSources = sheet.LinkedSources,
            LinkableRows = sheet.Tables
                .Where(table => tableIds.Contains(table.Id))
                .Select(table => new SheetTableLinkableRows { SheetTableId = table.Id, Rows = table.LinkableRows })
                .ToList(),
            TemplateCells = sheet.TemplateCells.Where(templateCell => templateCellIds.Contains(templateCell.Id)).ToList(),
            Sheet = sheet,
        };
    }

    private static void Collect(IReadOnlyList<SheetSection> sections, HashSet<int> wanted, List<SheetRow> rows)
    {
        foreach (var section in sections)
        {
            rows.AddRange(section.Rows.Where(row => wanted.Contains(row.Id)));
            Collect(section.Sections, wanted, rows);
        }
    }
}
