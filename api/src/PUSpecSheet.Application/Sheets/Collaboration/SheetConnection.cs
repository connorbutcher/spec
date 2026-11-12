namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>One live connection (a browser tab) and the sheet it has open.</summary>
public sealed record SheetConnection
{
    public SheetConnection(int sheetId, int userId, string displayName)
    {
        this.SheetId = sheetId;
        this.UserId = userId;
        this.DisplayName = displayName;
    }

    public int SheetId { get; init; }

    public int UserId { get; init; }

    public string DisplayName { get; init; }
}
