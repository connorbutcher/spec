namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// What someone joining a sheet needs to catch up: who has it open, and the open takeover requests they
/// made or have to answer.
/// </summary>
public sealed record SheetLiveStateDto(
    IReadOnlyList<SheetPresenceUserDto> Users,
    IReadOnlyList<RowTakeoverDto> Takeovers);
