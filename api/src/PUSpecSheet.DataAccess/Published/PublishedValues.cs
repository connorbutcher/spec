using System.Globalization;
using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Published;

public static class PublishedValues
{
    // Drops the trailing zeros the column's fixed scale adds (12.5000000000 becomes 12.5) by dividing by one with
    // the most decimal places a decimal holds.
    public static decimal Number(decimal value)
    {
        return value / 1.0000000000000000000000000000m;
    }

    public static string? Text(object value)
    {
        if (value is IReadOnlyDictionary<string, string> slots)
        {
            return slots.Values.FirstOrDefault();
        }

        return Convert.ToString(value, CultureInfo.InvariantCulture);
    }

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
