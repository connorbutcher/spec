using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.InstanceSettings;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>
/// A value saved into a linked dropdown must be one of the choices its column offers at that moment. A value
/// already in the cell is left alone if the column later changes: the row only changes when someone edits it.
/// </summary>
public sealed class LinkedDropdownValueRule(SheetReader reader)
{
    /// <summary>Expects each cell's template cell and cell type to be loaded.</summary>
    /// <exception cref="InvalidRequestException">A value isn't one of its cell's choices, or the cell has no column chosen.</exception>
    public async Task EnsureChoicesAsync(
        int sheetId,
        IReadOnlyDictionary<int, SheetCell> cellsById,
        IReadOnlyList<CellValueRequest> values,
        CancellationToken cancellationToken)
    {
        var linked = values
            .Where(value => !string.IsNullOrWhiteSpace(value.Text)
                && cellsById.TryGetValue(value.SheetCellId, out var cell)
                && cell.TemplateCell.CellType.Kind == CellKind.LinkedDropdown)
            .ToList();
        if (linked.Count == 0)
        {
            return;
        }

        // The sheet as the user sees it, so the choices are the ones the screen offered.
        var sheet = await reader.ReadLiveAsync(sheetId, cancellationToken);
        foreach (var value in linked)
        {
            var template = cellsById[value.SheetCellId].TemplateCell;
            var name = string.IsNullOrWhiteSpace(template.Caption) ? template.CellType.Name : template.Caption;
            var settings = SheetLinks.FindCell(sheet, value.SheetCellId)?.Settings as LinkedDropdownInstanceSettings;
            var options = SheetLinks.OptionsOf(sheet, settings)
                ?? throw new InvalidRequestException($"Choose the table and column '{name}' takes its choices from first.");
            if (!options.Contains(value.Text!.Trim(), StringComparer.Ordinal))
            {
                throw new InvalidRequestException($"That isn't one of the choices for '{name}'.");
            }
        }
    }
}
