namespace PUSpecSheet.Migrator.Building;

/// <summary>
/// Pairs up the items of one list in two consecutive copies, so an item keeps its identity from copy to copy.
/// Items with the same signature are paired first, keeping their order (a longest common subsequence, as a
/// text diff pairs unchanged lines). The items left between two such pairs are then paired in order, as
/// changed versions of each other; any extra on the old side were removed and any extra on the new side were
/// added. So inserting a row in the middle of a section leaves the rows around it alone.
/// </summary>
internal static class SiblingMatcher
{
    /// <summary>For each item of <paramref name="current"/>, the index of its partner in <paramref name="previous"/>, or -1 when it is new.</summary>
    /// <param name="previous">The previous copy's signatures, in display order.</param>
    /// <param name="current">The current copy's signatures, in display order.</param>
    public static int[] Match(IReadOnlyList<string> previous, IReadOnlyList<string> current)
    {
        var partners = new int[current.Count];
        Array.Fill(partners, -1);

        var anchors = CommonSubsequence(previous, current);

        // A sentinel anchor past both ends closes the last gap.
        anchors.Add((previous.Count, current.Count));
        var previousStart = 0;
        var currentStart = 0;
        foreach (var (previousIndex, currentIndex) in anchors)
        {
            var gap = Math.Min(previousIndex - previousStart, currentIndex - currentStart);
            for (var offset = 0; offset < gap; offset++)
            {
                partners[currentStart + offset] = previousStart + offset;
            }

            if (currentIndex < current.Count)
            {
                partners[currentIndex] = previousIndex;
            }

            previousStart = previousIndex + 1;
            currentStart = currentIndex + 1;
        }

        return partners;
    }

    private static List<(int Previous, int Current)> CommonSubsequence(IReadOnlyList<string> previous, IReadOnlyList<string> current)
    {
        // lengths[i, j]: the longest common subsequence of previous[i..] and current[j..].
        var lengths = new int[previous.Count + 1, current.Count + 1];
        for (var i = previous.Count - 1; i >= 0; i--)
        {
            for (var j = current.Count - 1; j >= 0; j--)
            {
                lengths[i, j] = string.Equals(previous[i], current[j], StringComparison.Ordinal)
                    ? lengths[i + 1, j + 1] + 1
                    : Math.Max(lengths[i + 1, j], lengths[i, j + 1]);
            }
        }

        var pairs = new List<(int Previous, int Current)>();
        var p = 0;
        var c = 0;
        while (p < previous.Count && c < current.Count)
        {
            if (string.Equals(previous[p], current[c], StringComparison.Ordinal))
            {
                pairs.Add((p, c));
                p++;
                c++;
            }
            else if (lengths[p + 1, c] >= lengths[p, c + 1])
            {
                p++;
            }
            else
            {
                c++;
            }
        }

        return pairs;
    }
}
