namespace PUSpecSheet.DataAccess.Sheets.Linking;

internal static class LinkedRowValues
{
    // The row's name, or null for a row with no values, which can't be linked to.
    public static string? NameOf(IReadOnlyList<string> values)
    {
        return values.Count == 0 ? null : values[0];
    }

    public static IEnumerable<string> ChoicesOf(IReadOnlyList<string> values)
    {
        return values.Skip(1);
    }
}
