using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>
/// Gathers the choices of the columns linked dropdowns are pointed at, as the sheet's visible cells are
/// walked top to bottom, and the row behind each choice for the columns a cell wants rows from. Only
/// columns something is linked to are kept, so a sheet with no linked dropdowns costs nothing.
/// </summary>
internal sealed class LinkedOptionCollector
{
    private readonly Dictionary<(int TableId, int TemplateCellId), List<string>> options = [];
    private readonly Dictionary<(int TableId, int TemplateCellId), List<SheetLinkedRowDto>> rows = [];

    /// <param name="values">Every cell value and setting the view shows.</param>
    public LinkedOptionCollector(IEnumerable<CellValueBag> values)
    {
        foreach (var settings in values.Select(value => value.Settings).OfType<LinkedDropdownInstanceSettings>())
        {
            if (settings is { SourceSheetTableId: { } tableId, SourceTemplateCellId: { } templateCellId })
            {
                options.TryAdd((tableId, templateCellId), []);
                if (settings.ShowsRowValues)
                {
                    rows.TryAdd((tableId, templateCellId), []);
                }
            }
        }
    }

    /// <summary>Whether anything is linked to this column, so its values are worth turning into text.</summary>
    public bool Wants(int tableId, int templateCellId)
    {
        return options.ContainsKey((tableId, templateCellId));
    }

    /// <summary>Whether a cell wants the rows behind the choices of any column of this table.</summary>
    public bool WantsRowsOf(int tableId)
    {
        return rows.Keys.Any(key => key.TableId == tableId);
    }

    /// <summary>Whether a cell wants the rows behind this column's choices.</summary>
    public bool WantsRows(int tableId, int templateCellId)
    {
        return rows.ContainsKey((tableId, templateCellId));
    }

    /// <summary>Adds a value of a column. A value the column already has is not repeated.</summary>
    public void Add(int tableId, int templateCellId, string text)
    {
        if (options.TryGetValue((tableId, templateCellId), out var known) && !known.Contains(text, StringComparer.Ordinal))
        {
            known.Add(text);
        }
    }

    /// <summary>Adds the row behind a choice. The first row to hold a value is the one that choice means.</summary>
    public void AddRow(int tableId, int templateCellId, SheetLinkedRowDto row)
    {
        if (rows.TryGetValue((tableId, templateCellId), out var known)
            && !known.Any(candidate => string.Equals(candidate.Option, row.Option, StringComparison.Ordinal)))
        {
            known.Add(row);
        }
    }

    public List<SheetLinkedSourceDto> Build()
    {
        return options
            .OrderBy(entry => entry.Key.TableId)
            .ThenBy(entry => entry.Key.TemplateCellId)
            .Select(entry => new SheetLinkedSourceDto(entry.Key.TableId, entry.Key.TemplateCellId, entry.Value)
            {
                Rows = rows.GetValueOrDefault(entry.Key) ?? [],
            })
            .ToList();
    }
}
