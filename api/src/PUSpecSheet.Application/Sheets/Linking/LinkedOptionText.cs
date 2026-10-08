using System.Globalization;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>How a cell's value reads as a choice in a linked dropdown.</summary>
internal static class LinkedOptionText
{
    /// <summary>The value as text, or null when the cell is empty or its kind can't be linked to.</summary>
    public static string? Of(CellValueBag? value, CellType cellType)
    {
        if (value is null)
        {
            return null;
        }

        var text = cellType.Kind switch
        {
            CellKind.Text or CellKind.LinkedDropdown => value.Text?.Trim(),

            // Without the trailing zeros the database's fixed scale adds: 12.5, not 12.5000000000.
            CellKind.Number => (value.Number / 1.0000000000000000000000000000m)?.ToString(CultureInfo.InvariantCulture),
            CellKind.Date => value.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            CellKind.TextDropdown or CellKind.NumberDropdown => cellType.Options.FirstOrDefault(option => option.Id == value.OptionId)?.Value,
            _ => null,
        };

        return string.IsNullOrEmpty(text) ? null : text;
    }
}
