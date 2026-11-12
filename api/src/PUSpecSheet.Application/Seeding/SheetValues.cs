using System.Globalization;
using System.Text.Json;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Seeding;

/// <summary>
/// Turns a plain list of values into the requests that fill a row's cells, left to right, skipping the
/// heading and group cells that hold no value. A string fills a text or linked cell or picks a dropdown
/// option by its text, a number fills a number cell or picks a number option, a date or a bool fills a
/// date or checkbox, and a dictionary of slot key to text fills a labelled text cell. Null leaves a cell as it is.
/// </summary>
public static class SheetValues
{
    /// <summary>
    /// The requests that put <paramref name="values"/> into the row's own cells (no <paramref name="columnBlockId"/>)
    /// or into its cells in one column block copy.
    /// </summary>
    public static List<CellValueRequest> For(
        SheetDto sheet,
        SheetRowDto row,
        int? columnBlockId,
        ShowcaseCellTypeCatalog cellTypes,
        params object?[] values)
    {
        var templateCells = sheet.TemplateCells.ToDictionary(cell => cell.Id);
        var cells = row.Cells
            .Where(cell => cell.SheetColumnBlockId == columnBlockId)
            .Select(cell => (Cell: cell, Template: templateCells[cell.TemplateCellId]))
            .Where(pair => cellTypes.KindOf(pair.Template.CellTypeId).StoresValue())
            .OrderBy(pair => pair.Template.Column)
            .ToList();
        if (values.Length > cells.Count)
        {
            throw new InvalidOperationException($"The row has {cells.Count} cells to fill but {values.Length} values were given.");
        }

        var requests = new List<CellValueRequest>();
        for (var index = 0; index < values.Length; index++)
        {
            if (values[index] is { } value)
            {
                requests.Add(Request(cells[index].Cell.Id, cells[index].Template.CellTypeId, cellTypes, value));
            }
        }

        return requests;
    }

    private static CellValueRequest Request(int cellId, int cellTypeId, ShowcaseCellTypeCatalog cellTypes, object value)
    {
        var kind = cellTypes.KindOf(cellTypeId);
        object stored = (kind, value) switch
        {
            (CellKind.TextDropdown or CellKind.NumberDropdown, _) => cellTypes.OptionId(cellTypeId, OptionText(value)),
            (CellKind.Number, decimal or int) => value,
            (CellKind.Date, DateOnly) => value,
            (CellKind.Checkbox, bool) => value,
            (CellKind.Text or CellKind.LinkedDropdown, string) => value,
            (CellKind.LabelledText, IReadOnlyDictionary<string, string>) => value,
            _ => throw new InvalidOperationException($"A {value.GetType().Name} can't go in a {kind} cell."),
        };
        return new CellValueRequest(cellId, JsonSerializer.SerializeToElement(stored));
    }

    private static string OptionText(object value)
    {
        return value switch
        {
            decimal number => number.ToString("0.######", CultureInfo.InvariantCulture),
            int number => number.ToString(CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
    }
}
