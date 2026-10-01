using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Display orders on sheets are spaced out so that moving one item only needs a new value for that
/// item, not for its siblings (which would lock them all). Items go between their new neighbours.
/// </summary>
internal static class OrderGaps
{
    public const int Spacing = 1024;

    /// <summary>The order for a new item placed after <paramref name="existingOrders"/>.</summary>
    public static int Next(IEnumerable<int> existingOrders)
    {
        var orders = existingOrders.ToList();
        return orders.Count == 0 ? Spacing : orders.Max() + Spacing;
    }

    /// <summary>
    /// The order for an item moved to a 1-based <paramref name="position"/> among siblings whose orders
    /// (without the moved item) are <paramref name="siblingOrders"/>, ascending.
    /// </summary>
    public static int PlaceAt(IReadOnlyList<int> siblingOrders, int position)
    {
        var index = Math.Clamp(position - 1, 0, siblingOrders.Count);
        int? before = index > 0 ? siblingOrders[index - 1] : null;
        int? after = index < siblingOrders.Count ? siblingOrders[index] : null;
        return Between(before, after);
    }

    private static int Between(int? before, int? after)
    {
        if (before is null && after is null)
        {
            return Spacing;
        }

        if (before is null)
        {
            return after!.Value - Spacing;
        }

        if (after is null)
        {
            return before.Value + Spacing;
        }

        var gap = after.Value - before.Value;
        if (gap < 2)
        {
            throw new ConflictException("There's no room to put it there. Refresh the sheet and try again.");
        }

        return before.Value + (gap / 2);
    }
}
