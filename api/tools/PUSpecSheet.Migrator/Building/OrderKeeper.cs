namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Gives one parent's items their display orders in a copy, keeping as many of the orders they already have as
/// still fit, as the app does. Legacy items have no order of their own, only a position. Numbering each copy
/// afresh would give every row below an inserted one a new order, and so a new revision, though nothing about it
/// changed. Instead, the longest run of items whose existing orders are already in the right order keeps them,
/// and the rest are placed between their neighbours, <see cref="Spacing"/> apart where there is room.
/// Only when there is no room at all is the whole list numbered afresh.
/// </summary>
internal static class OrderKeeper
{
    /// <summary>
    /// How far apart new items are placed, as the app places them.
    /// </summary>
    public const int Spacing = 1024;

    /// <param name="existing">For each item, in its place in the copy, its current order, or null when it is new or was removed.</param>
    /// <returns>Each item's order, rising in the same sequence.</returns>
    public static int[] Assign(IReadOnlyList<int?> existing)
    {
        var orders = new int?[existing.Count];
        foreach (var index in LongestRisingRun(existing))
        {
            orders[index] = existing[index];
        }

        var start = 0;
        while (start < orders.Length)
        {
            if (orders[start] is not null)
            {
                start++;
                continue;
            }

            var end = start;
            while (end < orders.Length && orders[end] is null)
            {
                end++;
            }

            int? before = start > 0 ? orders[start - 1] : null;
            int? after = end < orders.Length ? orders[end] : null;
            if (!Fill(orders, start, end, before, after))
            {
                return Renumbered(existing.Count);
            }

            start = end;
        }

        return orders.Select(order => order!.Value).ToArray();
    }

    /// <summary>Orders for the gap <c>[start, end)</c> between two kept orders; false when they don't fit.</summary>
    private static bool Fill(int?[] orders, int start, int end, int? before, int? after)
    {
        var count = end - start;
        for (var offset = 0; offset < count; offset++)
        {
            long order = (before, after) switch
            {
                (null, null) => (long)(offset + 1) * Spacing,
                (null, { } next) => next - ((long)(count - offset) * Spacing),
                ({ } previous, null) => previous + ((long)(offset + 1) * Spacing),
                ({ } previous, { } next) when next - previous > count => previous + ((long)(next - previous) * (offset + 1) / (count + 1)),
                _ => long.MinValue,
            };

            if (order is < int.MinValue or > int.MaxValue)
            {
                return false;
            }

            orders[start + offset] = (int)order;
        }

        return true;
    }

    /// <summary>The indexes of the longest run of items, in sequence, whose existing orders strictly rise.</summary>
    private static List<int> LongestRisingRun(IReadOnlyList<int?> existing)
    {
        var indexes = Enumerable.Range(0, existing.Count).Where(index => existing[index] is not null).ToList();
        var lengths = new int[indexes.Count];
        var previous = new int[indexes.Count];
        var bestEnd = -1;
        for (var i = 0; i < indexes.Count; i++)
        {
            lengths[i] = 1;
            previous[i] = -1;
            for (var j = 0; j < i; j++)
            {
                if (existing[indexes[j]] < existing[indexes[i]] && lengths[j] + 1 > lengths[i])
                {
                    lengths[i] = lengths[j] + 1;
                    previous[i] = j;
                }
            }

            if (bestEnd < 0 || lengths[i] > lengths[bestEnd])
            {
                bestEnd = i;
            }
        }

        var run = new List<int>();
        for (var i = bestEnd; i >= 0; i = previous[i])
        {
            run.Add(indexes[i]);
        }

        run.Reverse();
        return run;
    }

    private static int[] Renumbered(int count)
    {
        return Enumerable.Range(1, count).Select(place => place * Spacing).ToArray();
    }
}
