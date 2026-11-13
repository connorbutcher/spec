using System.Globalization;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>How a cell's value reads as a choice in a linked dropdown.</summary>
internal static class LinkedOptionText
{
    /// <summary>The value as text, or null when the cell is empty or its kind can't be linked to.</summary>
    /// <param name="value">The cell's value.</param>
    /// <param name="cellType">The cell's type, with its options.</param>
    /// <param name="cellOverride">The settings the cell changes from its type's, which can set a number's decimal places.</param>
    public static string? Of(CellValueBag? value, DbCellType cellType, CellConfiguration? cellOverride = null)
    {
        if (value is null)
        {
            return null;
        }

        var text = cellType.Kind switch
        {
            CellKind.Text or CellKind.LinkedDropdown => value.Text?.Trim(),
            CellKind.Number => Number(value.Number, cellType.Configuration.Apply(cellOverride)),
            CellKind.Date => value.Date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            CellKind.TextDropdown or CellKind.NumberDropdown => cellType.Options.FirstOrDefault(option => option.Id == value.OptionId)?.Value,
            _ => null,
        };

        return string.IsNullOrEmpty(text) ? null : text;
    }

    /// <summary>
    /// A number as its cell shows it: to the cell's decimal places when it has them (0.80), and otherwise
    /// without the trailing zeros the database's fixed scale adds (12.5, not 12.5000000000). No unit.
    /// </summary>
    private static string? Number(decimal? number, CellConfiguration configuration)
    {
        if (number is not { } value)
        {
            return null;
        }

        return configuration is NumberCellConfiguration { DecimalPlaces: { } places }
            ? value.ToString("F" + places.ToString(CultureInfo.InvariantCulture), CultureInfo.InvariantCulture)
            : (value / 1.0000000000000000000000000000m).ToString(CultureInfo.InvariantCulture);
    }
}
