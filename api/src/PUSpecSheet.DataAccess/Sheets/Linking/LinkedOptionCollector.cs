using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Values;

namespace PUSpecSheet.DataAccess.Sheets.Linking;

/// <summary>
/// Gathers the choices of the columns and rows linked dropdowns are pointed at, as the sheet's visible
/// cells are walked top to bottom. Only what something is linked to is kept, so a sheet with no linked
/// dropdowns costs nothing.
/// </summary>
internal sealed class LinkedOptionCollector
{
    private readonly Dictionary<(int TableId, int TemplateCellId), List<string>> _columns = [];
    private readonly Dictionary<(int TableId, int RowId), List<string>> _rows = [];

    /// <param name="values">Every cell value and setting the view shows.</param>
    public LinkedOptionCollector(IEnumerable<CellValueBag> values)
    {
        foreach (var settings in values.Select(value => value.Settings).OfType<LinkedDropdownInstanceSettings>())
        {
            if (!settings.HasSource)
            {
                continue;
            }

            if (settings.SourceTemplateCellId is { } templateCellId)
            {
                _columns.TryAdd((settings.SourceSheetTableId!.Value, templateCellId), []);
            }
            else
            {
                _rows.TryAdd((settings.SourceSheetTableId!.Value, settings.SourceSheetRowId!.Value), []);
            }
        }
    }

    /// <summary>Whether anything is linked to this column, so its values are worth turning into text.</summary>
    public bool Wants(int tableId, int templateCellId)
    {
        return _columns.ContainsKey((tableId, templateCellId));
    }

    public bool WantsRow(int tableId, int rowId)
    {
        return _rows.ContainsKey((tableId, rowId));
    }

    /// <summary>Adds a value of a column. A value the column already has is not repeated.</summary>
    public void Add(int tableId, int templateCellId, string text)
    {
        if (_columns.TryGetValue((tableId, templateCellId), out var known) && !known.Contains(text, StringComparer.Ordinal))
        {
            known.Add(text);
        }
    }

    /// <summary>Adds a value of a row. A value the row already has is not repeated.</summary>
    public void AddToRow(int tableId, int rowId, string text)
    {
        if (_rows.TryGetValue((tableId, rowId), out var known) && !known.Contains(text, StringComparer.Ordinal))
        {
            known.Add(text);
        }
    }

    public List<SheetLinkedSource> Build()
    {
        var ofColumns = _columns
            .OrderBy(entry => entry.Key.TableId)
            .ThenBy(entry => entry.Key.TemplateCellId)
            .Select(entry => new SheetLinkedSource
            {
                SheetTableId = entry.Key.TableId,
                TemplateCellId = entry.Key.TemplateCellId,
                Options = entry.Value,
            });
        var ofRows = _rows
            .OrderBy(entry => entry.Key.TableId)
            .ThenBy(entry => entry.Key.RowId)
            .Select(entry => new SheetLinkedSource
            {
                SheetTableId = entry.Key.TableId,
                SheetRowId = entry.Key.RowId,
                Options = entry.Value,
            });
        return [.. ofColumns, .. ofRows];
    }
}
