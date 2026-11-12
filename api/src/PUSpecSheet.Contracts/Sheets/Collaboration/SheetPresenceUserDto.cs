namespace PUSpecSheet.Contracts.Sheets.Collaboration;

/// <summary>
/// Someone who has a sheet open right now. A person with the sheet open in several tabs or windows is
/// listed once, with how many of them there are.
/// </summary>
public sealed record SheetPresenceUserDto
{
    public SheetPresenceUserDto(int userId, string displayName, int connectionCount)
    {
        this.UserId = userId;
        this.DisplayName = displayName;
        this.ConnectionCount = connectionCount;
    }

    public int UserId { get; init; }

    public string DisplayName { get; init; }

    public int ConnectionCount { get; init; }
}
