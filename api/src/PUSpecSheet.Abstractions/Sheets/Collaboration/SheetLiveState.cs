namespace PUSpecSheet.Abstractions.Sheets.Collaboration;

/// <summary>
/// What someone joining a sheet needs to catch up: who has it open, the rows people are in but have not
/// changed yet, and the open takeover requests they made or have to answer.
/// </summary>
public sealed record SheetLiveState
{
    public required IReadOnlyList<SheetPresenceUser> Users { get; init; }

    public required IReadOnlyList<RowCheckout> Checkouts { get; init; }

    public required IReadOnlyList<RowTakeover> Takeovers { get; init; }
}
