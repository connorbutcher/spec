namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>A row that is checked out: the sheet it is on and who holds its draft.</summary>
public sealed record RowCheckout(int SheetId, int HolderUserId);
