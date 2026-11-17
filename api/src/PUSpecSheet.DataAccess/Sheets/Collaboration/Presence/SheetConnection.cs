namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

/// <summary>One live connection (a browser tab) and the sheet it has open.</summary>
public sealed record SheetConnection
{
    public SheetConnection(int sheetId, int userId, string displayName)
    {
        SheetId = sheetId;
        UserId = userId;
        DisplayName = displayName;
    }

    public int SheetId { get; init; }

    public int UserId { get; init; }

    public string DisplayName { get; init; }
}
