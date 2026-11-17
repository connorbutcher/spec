namespace PUSpecSheet.DataAccess.Sheets.Drafts;

/// <summary>An item's current published revision and any draft on it, as the draft gateways load them.</summary>
public sealed record DraftState<TRevision>
    where TRevision : class
{
    public DraftState(TRevision? current, TRevision? draft)
    {
        Current = current;
        Draft = draft;
    }

    public TRevision? Current { get; init; }

    public TRevision? Draft { get; init; }

    /// <summary>True when the item has no published revision, so it only exists as someone's draft.</summary>
    public bool IsNew => Current is null;
}
