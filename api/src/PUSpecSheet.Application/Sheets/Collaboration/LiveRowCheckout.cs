namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>A row one person is in, on one sheet, without having changed it yet.</summary>
public sealed record LiveRowCheckout(int SheetId, int RowId, int UserId, string DisplayName);
