using System.Text.RegularExpressions;

namespace PUSpecSheet.Domain.CellTypes.LabelledText;

/// <summary>The rules every list of labelled text slots follows, wherever it is set.</summary>
public static partial class LabelledTextRules
{
    public const int MostSlots = 8;

    public const int LongestKey = 20;

    public const int LongestLabel = 20;

    /// <summary>The slots a cell has when neither its type nor the sheet names any: one value with no label.</summary>
    public static readonly IReadOnlyList<LabelledTextSlot> DefaultSlots = [new("value", string.Empty)];

    /// <summary>Why the slots are invalid, or null when they're fine. Null slots mean "not set" and are fine.</summary>
    public static string? CheckSlots(IReadOnlyList<LabelledTextSlot>? slots)
    {
        if (slots is null)
        {
            return null;
        }

        if (slots.Count is < 1 or > MostSlots)
        {
            return $"A labelled text cell needs between 1 and {MostSlots} values.";
        }

        if (slots.Any(slot => !IsKey(slot.Key)))
        {
            return $"Each value needs a key of up to {LongestKey} lower-case letters and digits, starting with a letter.";
        }

        if (slots.Select(slot => slot.Key).Distinct(StringComparer.Ordinal).Count() != slots.Count)
        {
            return "Each value needs its own key.";
        }

        if (slots.Any(slot => slot.Label is null || slot.Label.Length > LongestLabel))
        {
            return $"Labels can be at most {LongestLabel} characters.";
        }

        return null;
    }

    /// <summary>Whether two lists of slots are the same, slot for slot and in the same order.</summary>
    public static bool Same(IReadOnlyList<LabelledTextSlot>? left, IReadOnlyList<LabelledTextSlot>? right)
    {
        if (left is null || right is null)
        {
            return left is null && right is null;
        }

        return left.SequenceEqual(right);
    }

    /// <summary>A hash that agrees with <see cref="Same"/>.</summary>
    public static int HashOf(IReadOnlyList<LabelledTextSlot>? slots)
    {
        var hash = default(HashCode);
        foreach (var slot in slots ?? [])
        {
            hash.Add(slot);
        }

        return hash.ToHashCode();
    }

    public static bool IsKey(string? key)
    {
        return key is not null && SlotKey().IsMatch(key);
    }

    public static bool IsColor(string? value)
    {
        return value is null || HexColor().IsMatch(value);
    }

    [GeneratedRegex("^[a-z][a-z0-9]{0,19}$")]
    private static partial Regex SlotKey();

    [GeneratedRegex("^#[0-9a-fA-F]{6}$")]
    private static partial Regex HexColor();
}
