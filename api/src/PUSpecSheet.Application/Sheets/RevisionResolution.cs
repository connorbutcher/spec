namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// What to show for one table, section or row: <see cref="Shown"/> is the viewer's own draft if they
/// have one, otherwise the published revision; <see cref="Draft"/> is any draft at all, which is the
/// item's lock. An item with no <see cref="Shown"/> revision isn't visible to the viewer.
/// </summary>
public sealed record RevisionResolution<TRevision>(TRevision? Shown, TRevision? Draft)
    where TRevision : class;
