using PUSpecSheet.DataAccess.Sheets.Revisions;

namespace PUSpecSheet.DataAccess.Sheets.Values;

public static class SlotValues
{
    public static Dictionary<string, string>? Clean(IReadOnlyDictionary<string, string>? slots)
    {
        if (slots is null)
        {
            return null;
        }

        var cleaned = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (key, value) in slots)
        {
            var text = value?.Trim();
            if (!string.IsNullOrEmpty(text))
            {
                cleaned[key] = text;
            }
        }

        return cleaned.Count == 0 ? null : cleaned;
    }

    // Whether two cells hold the same values slot for slot, the way a person would see it: an empty slot is no value
    // and surrounding spaces don't count. The order isn't compared: that comes from the slots, which are the cell's
    // settings and compared with them.
    public static bool Same(IReadOnlyDictionary<string, string>? left, IReadOnlyDictionary<string, string>? right)
    {
        var leftValues = Clean(left) ?? [];
        var rightValues = Clean(right) ?? [];
        return leftValues.Count == rightValues.Count
            && leftValues.All(entry => rightValues.TryGetValue(entry.Key, out var other) && SheetRevisionComparer.SameText(entry.Value, other));
    }
}
