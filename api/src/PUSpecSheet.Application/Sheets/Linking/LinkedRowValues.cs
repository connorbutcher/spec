namespace PUSpecSheet.Application.Sheets.Linking;

/// <summary>
/// Splits a row's values into what the row is called and what it offers a linked dropdown pointed at it.
/// The first value, left to right, names the row ("Pin offset (mm)"); the rest are the choices.
/// </summary>
internal static class LinkedRowValues
{
    /// <summary>The row's name, or null for a row with no values, which can't be linked to.</summary>
    /// <param name="values">The row's values as text, left to right, without the empty cells.</param>
    public static string? NameOf(IReadOnlyList<string> values)
    {
        return values.Count == 0 ? null : values[0];
    }

    /// <summary>The values a dropdown pointed at the row offers: every value after the one that names it.</summary>
    public static IEnumerable<string> ChoicesOf(IReadOnlyList<string> values)
    {
        return values.Skip(1);
    }
}
