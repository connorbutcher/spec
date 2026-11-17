namespace PUSpecSheet.DataAccess.Common;

internal static class DisplayOrdering
{
    public static void Move<T>(IEnumerable<T> siblings, T item, int position, Func<T, int> getOrder, Action<T, int> setOrder)
        where T : class
    {
        var ordered = siblings
            .Where(sibling => !ReferenceEquals(sibling, item))
            .OrderBy(getOrder)
            .ToList();

        var index = Math.Clamp(position - 1, 0, ordered.Count);
        ordered.Insert(index, item);
        RenumberAsListed(ordered, setOrder);
    }

    public static void Renumber<T>(IEnumerable<T> siblings, Func<T, int> getOrder, Action<T, int> setOrder)
    {
        RenumberAsListed(siblings.OrderBy(getOrder).ToList(), setOrder);
    }

    public static void RenumberAsListed<T>(IReadOnlyList<T> ordered, Action<T, int> setOrder)
    {
        for (var index = 0; index < ordered.Count; index++)
        {
            setOrder(ordered[index], index + 1);
        }
    }
}
