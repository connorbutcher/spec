namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

/// <summary>One live connection (a browser tab) and the sheet it has open.</summary>
public sealed record SheetConnection
{
    public required int SheetId { get; init; }

    public required int UserId { get; init; }

    public required string DisplayName { get; init; }
}
