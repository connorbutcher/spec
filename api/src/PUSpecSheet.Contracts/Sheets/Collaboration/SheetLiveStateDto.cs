namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// What someone joining a sheet needs to catch up: who has it open, the rows people are in but have not
/// changed yet, and the open takeover requests they made or have to answer.
/// </summary>
public sealed record SheetLiveStateDto
{
    public SheetLiveStateDto(
        IReadOnlyList<SheetPresenceUserDto> users,
        IReadOnlyList<RowCheckoutDto> checkouts,
        IReadOnlyList<RowTakeoverDto> takeovers)
    {
        this.Users = users;
        this.Checkouts = checkouts;
        this.Takeovers = takeovers;
    }

    public IReadOnlyList<SheetPresenceUserDto> Users { get; init; }

    public IReadOnlyList<RowCheckoutDto> Checkouts { get; init; }

    public IReadOnlyList<RowTakeoverDto> Takeovers { get; init; }
}
