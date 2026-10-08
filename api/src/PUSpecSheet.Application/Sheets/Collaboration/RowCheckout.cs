namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// A row that is checked out: the sheet it is on, who holds its draft, and whether that draft differs from
/// what is published. A row with unpublished changes stays with its holder until they publish or discard.
/// </summary>
public sealed record RowCheckout(int SheetId, int HolderUserId, bool HasChanges);
