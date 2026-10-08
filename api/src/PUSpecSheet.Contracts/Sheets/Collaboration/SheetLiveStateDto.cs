namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// What someone joining a sheet needs to catch up: who has it open, the rows people are in but have not
/// changed yet, and the open takeover requests they made or have to answer.
/// </summary>
public sealed record SheetLiveStateDto(
    IReadOnlyList<SheetPresenceUserDto> Users,
    IReadOnlyList<RowCheckoutDto> Checkouts,
    IReadOnlyList<RowTakeoverDto> Takeovers);
