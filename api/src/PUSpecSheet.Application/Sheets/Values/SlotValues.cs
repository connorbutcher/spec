using PUSpecSheet.Application.Sheets.Revisions;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>The values of a labelled text cell, by slot key, as they are saved and compared.</summary>
public static class SlotValues
{
    /// <summary>The values with surrounding spaces taken off and the empty ones left out, in the order given; null when none are left.</summary>
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

    /// <summary>
    /// Whether two cells hold the same values slot for slot, the way a person would see it: an empty slot is
    /// no value and surrounding spaces don't count. The order isn't compared: that comes from the slots, which
    /// are the cell's settings and compared with them.
    /// </summary>
    public static bool Same(IReadOnlyDictionary<string, string>? left, IReadOnlyDictionary<string, string>? right)
    {
        var leftValues = Clean(left) ?? [];
        var rightValues = Clean(right) ?? [];
        return leftValues.Count == rightValues.Count
            && leftValues.All(entry => rightValues.TryGetValue(entry.Key, out var other) && SheetRevisionComparer.SameText(entry.Value, other));
    }
}
