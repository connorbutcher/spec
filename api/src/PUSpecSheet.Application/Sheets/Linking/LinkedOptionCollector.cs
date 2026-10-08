using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>
/// Gathers the choices of the columns linked dropdowns are pointed at, as the sheet's visible cells are
/// walked top to bottom. Only columns something is linked to are kept, so a sheet with no linked
/// dropdowns costs nothing.
/// </summary>
internal sealed class LinkedOptionCollector
{
    private readonly Dictionary<(int TableId, int TemplateCellId), List<string>> options = [];

    /// <param name="values">Every cell value and setting the view shows.</param>
    public LinkedOptionCollector(IEnumerable<CellValueBag> values)
    {
        foreach (var settings in values.Select(value => value.Settings).OfType<LinkedDropdownInstanceSettings>())
        {
            if (settings is { SourceSheetTableId: { } tableId, SourceTemplateCellId: { } templateCellId })
            {
                options.TryAdd((tableId, templateCellId), []);
            }
        }
    }

    /// <summary>Whether anything is linked to this column, so its values are worth turning into text.</summary>
    public bool Wants(int tableId, int templateCellId)
    {
        return options.ContainsKey((tableId, templateCellId));
    }

    /// <summary>Adds a value of a column. A value the column already has is not repeated.</summary>
    public void Add(int tableId, int templateCellId, string text)
    {
        if (options.TryGetValue((tableId, templateCellId), out var known) && !known.Contains(text, StringComparer.Ordinal))
        {
            known.Add(text);
        }
    }

    public List<SheetLinkedSourceDto> Build()
    {
        return options
            .OrderBy(entry => entry.Key.TableId)
            .ThenBy(entry => entry.Key.TemplateCellId)
            .Select(entry => new SheetLinkedSourceDto(entry.Key.TableId, entry.Key.TemplateCellId, entry.Value))
            .ToList();
    }
}
