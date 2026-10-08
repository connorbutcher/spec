using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>
/// Questions about linked dropdowns asked of a sheet as its viewer sees it, so a save is checked against
/// exactly the tables, columns and choices the screen offered.
/// </summary>
internal static class SheetLinks
{
    /// <summary>Whether the table and column are on the sheet and can be linked to.</summary>
    public static bool SourceExists(SheetDto sheet, LinkedDropdownInstanceSettings settings)
    {
        return sheet.Tables.Any(table => table.Id == settings.SourceSheetTableId
            && table.LinkableColumns.Any(column => column.TemplateCellId == settings.SourceTemplateCellId));
    }

    /// <summary>The choices the settings give, or null when their table or column isn't on the sheet.</summary>
    public static IReadOnlyList<string>? OptionsOf(SheetDto sheet, LinkedDropdownInstanceSettings? settings)
    {
        if (settings is null || !SourceExists(sheet, settings))
        {
            return null;
        }

        return sheet.LinkedSources
            .FirstOrDefault(source => source.SheetTableId == settings.SourceSheetTableId
                && source.TemplateCellId == settings.SourceTemplateCellId)
            ?.Options ?? [];
    }

    /// <summary>The cell with the given id, wherever it is on the sheet.</summary>
    public static SheetCellDto? FindCell(SheetDto sheet, int sheetCellId)
    {
        return sheet.Tables
            .SelectMany(table => table.Sections)
            .SelectMany(CellsOf)
            .FirstOrDefault(cell => cell.Id == sheetCellId);
    }

    private static IEnumerable<SheetCellDto> CellsOf(SheetSectionDto section)
    {
        return section.Rows
            .SelectMany(row => row.Cells)
            .Concat(section.Sections.SelectMany(CellsOf));
    }
}
