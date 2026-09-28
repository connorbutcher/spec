namespace PUSpecSheet.Application.Common;

/// <summary>Keeps sibling display orders as a gapless 1-based sequence.</summary>
internal static class DisplayOrdering
{
    /// <summary>
    /// Moves <paramref name="item"/> to a 1-based <paramref name="position"/> among
    /// <paramref name="siblings"/> (which includes it), then renumbers them all.
    /// </summary>
    public static void Move<T>(IEnumerable<T> siblings, T item, int position, Func<T, int> getOrder, Action<T, int> setOrder)
        where T : class
    {
        var ordered = siblings
            .Where(sibling => !ReferenceEquals(sibling, item))
            .OrderBy(getOrder)
            .ToList();

        var index = Math.Clamp(position - 1, 0, ordered.Count);
        ordered.Insert(index, item);
        Renumber(ordered, setOrder);
    }

    /// <summary>Renumbers <paramref name="siblings"/> 1..n in their current order.</summary>
    public static void Renumber<T>(IEnumerable<T> siblings, Func<T, int> getOrder, Action<T, int> setOrder)
    {
        Renumber(siblings.OrderBy(getOrder).ToList(), setOrder);
    }

    private static void Renumber<T>(IReadOnlyList<T> ordered, Action<T, int> setOrder)
    {
        for (var index = 0; index < ordered.Count; index++)
        {
            setOrder(ordered[index], index + 1);
        }
    }
}
