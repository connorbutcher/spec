namespace PUSpecSheet.Application.Sheets;

/// <summary>An item's current published revision and any draft on it, as the draft gateways load them.</summary>
public sealed record DraftState<TRevision>(TRevision? Current, TRevision? Draft)
    where TRevision : class
{
    /// <summary>True when the item has no published revision, so it only exists as someone's draft.</summary>
    public bool IsNew => Current is null;
}
