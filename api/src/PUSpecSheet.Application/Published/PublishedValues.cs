using System.Globalization;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Published;

/// <summary>Turns stored cell values into what a caller receives.</summary>
public static class PublishedValues
{
    /// <summary>
    /// Drops the trailing zeros the database's fixed scale adds, so 12.5 is sent as 12.5 and not
    /// 12.5000000000. Dividing by one written with the most decimal places a decimal can hold does that.
    /// </summary>
    public static decimal Number(decimal value)
    {
        return value / 1.0000000000000000000000000000m;
    }

    /// <summary>A dropdown's chosen option: a number for a number dropdown when its text is one, otherwise the text.</summary>
    public static object Option(string text, CellKind kind)
    {
        if (kind == CellKind.NumberDropdown
            && decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out var number))
        {
            return Number(number);
        }

        return text;
    }
}
