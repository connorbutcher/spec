namespace PUSpecSheet.DataAccess.Sheets.Revisions;

// What to show for one table, section or row: Shown is the viewer's own draft if they have one, otherwise the
// published revision; Draft is any draft at all, which is the item's lock. An item with no Shown revision isn't
// visible to the viewer.
public sealed record RevisionResolution<TRevision>
    where TRevision : class
{
    public TRevision? Shown { get; init; }

    public TRevision? Draft { get; init; }
}
