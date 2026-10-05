namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Decides whether a group of siblings has been put back in the order that is published, even though their
/// numeric display orders differ from the published ones. Moving A to the end, then B to the end, then C to
/// the end gives every item a new number but leaves them in their original order, so nothing has changed.
/// </summary>
public static class OrderRestoration
{
    /// <summary>
    /// Whether the items that are published and still shown are in the same order now as when published.
    /// <paramref name="items"/> gives each sibling's id, its published order (null if it has never been
    /// published), its order as the user sees it now, and whether the user has removed it.
    /// </summary>
    /// <returns>
    /// False when the order differs, and also when the group holds an item that has never been published:
    /// such an item has no original place, so the numbers are left as they are.
    /// </returns>
    public static bool IsRestored(IReadOnlyCollection<(int Id, int? PublishedOrder, int ShownOrder, bool Removed)> items)
    {
        if (items.Any(item => item.PublishedOrder is null && !item.Removed))
        {
            return false;
        }

        var kept = items.Where(item => item.PublishedOrder is not null && !item.Removed).ToList();
        var published = kept.OrderBy(item => item.PublishedOrder).ThenBy(item => item.Id).Select(item => item.Id);
        var shown = kept.OrderBy(item => item.ShownOrder).ThenBy(item => item.Id).Select(item => item.Id);
        return published.SequenceEqual(shown);
    }
}
