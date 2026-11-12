namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>A row one person is in, on one sheet, without having changed it yet.</summary>
public sealed record LiveRowCheckout
{
    public LiveRowCheckout(int sheetId, int rowId, int userId, string displayName)
    {
        this.SheetId = sheetId;
        this.RowId = rowId;
        this.UserId = userId;
        this.DisplayName = displayName;
    }

    public int SheetId { get; init; }

    public int RowId { get; init; }

    public int UserId { get; init; }

    public string DisplayName { get; init; }
}
