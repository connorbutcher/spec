namespace PUSpecSheet.DataAccess.Sheets.Drafts;

public sealed record DraftState<TRevision>
    where TRevision : class
{
    public TRevision? Current { get; init; }

    public TRevision? Draft { get; init; }

    // True when the item has no published revision, so it only exists as someone's draft.
    public bool IsNew => Current is null;
}
