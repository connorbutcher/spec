using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Linking;

// Questions about linked dropdowns asked of a sheet as its viewer sees it, so a save is checked against exactly the
// tables, columns, rows and choices the screen offered.
internal static class SheetLinks
{
    public static bool SourceExists(Sheet sheet, LinkedDropdownInstanceSettings settings)
    {
        var table = sheet.Tables.FirstOrDefault(candidate => candidate.Id == settings.SourceSheetTableId);
        if (table is null || !settings.HasSource)
        {
            return false;
        }

        return settings.SourceSheetRowId is { } rowId
            ? table.LinkableRows.Any(row => row.SheetRowId == rowId)
            : table.LinkableColumns.Any(column => column.TemplateCellId == settings.SourceTemplateCellId);
    }

    public static IReadOnlyList<string>? OptionsOf(Sheet sheet, LinkedDropdownInstanceSettings? settings)
    {
        if (settings is null || !SourceExists(sheet, settings))
        {
            return null;
        }

        return sheet.LinkedSources
            .FirstOrDefault(source => source.SheetTableId == settings.SourceSheetTableId
                && source.TemplateCellId == settings.SourceTemplateCellId
                && source.SheetRowId == settings.SourceSheetRowId)
            ?.Options ?? [];
    }

    public static SheetCell? FindCell(Sheet sheet, int sheetCellId)
    {
        return sheet.Tables
            .SelectMany(table => table.Sections)
            .SelectMany(CellsOf)
            .FirstOrDefault(cell => cell.Id == sheetCellId);
    }

    private static IEnumerable<SheetCell> CellsOf(SheetSection section)
    {
        return section.Rows
            .SelectMany(row => row.Cells)
            .Concat(section.Sections.SelectMany(CellsOf));
    }
}
