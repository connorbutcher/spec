using System.Text.Json;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.Migrator.Snapshots;

// The JSON stored on table and section revisions, written the same way every time so that equal contents are equal
// text and an unchanged table doesn't look changed.
internal static class SnapshotJson
{
    private static readonly JsonSerializerOptions _options = new(JsonSerializerDefaults.Web);

    public static string? WidgetValues(IReadOnlyDictionary<int, string?> values)
    {
        if (values.Count == 0)
        {
            return null;
        }

        var ordered = values.OrderBy(pair => pair.Key).ToDictionary(pair => pair.Key.ToString(System.Globalization.CultureInfo.InvariantCulture), pair => pair.Value);
        return JsonSerializer.Serialize(ordered, _options);
    }

    public static string? Notes(IReadOnlyList<SheetTableNote> notes)
    {
        return notes.Count == 0 ? null : JsonSerializer.Serialize(notes, _options);
    }
}
