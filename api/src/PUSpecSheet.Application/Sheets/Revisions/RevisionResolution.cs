namespace PUSpecSheet.Application.Sheets.Revisions;

/// <summary>
/// What to show for one table, section or row: <see cref="Shown"/> is the viewer's own draft if they
/// have one, otherwise the published revision; <see cref="Draft"/> is any draft at all, which is the
/// item's lock. An item with no <see cref="Shown"/> revision isn't visible to the viewer.
/// </summary>
public sealed record RevisionResolution<TRevision>
    where TRevision : class
{
    public RevisionResolution(TRevision? shown, TRevision? draft)
    {
        this.Shown = shown;
        this.Draft = draft;
    }

    public TRevision? Shown { get; init; }

    public TRevision? Draft { get; init; }
}
