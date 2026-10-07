namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// Someone who has a sheet open right now. A person with the sheet open in several tabs or windows is
/// listed once, with how many of them there are.
/// </summary>
public sealed record SheetPresenceUserDto(int UserId, string DisplayName, int ConnectionCount);
