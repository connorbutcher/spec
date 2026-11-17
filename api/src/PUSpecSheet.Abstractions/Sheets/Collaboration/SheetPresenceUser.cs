namespace PUSpecSheet.Abstractions.Sheets.Collaboration;

/// <summary>
/// Someone who has a sheet open right now. A person with the sheet open in several tabs or windows is
/// listed once, with how many of them there are.
/// </summary>
public sealed record SheetPresenceUser
{
    public required int UserId { get; init; }

    public required string DisplayName { get; init; }

    public required int ConnectionCount { get; init; }
}
