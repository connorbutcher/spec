namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

/// <summary>
/// A row that is checked out: the sheet it is on, who holds its draft, and whether that draft differs from
/// what is published. A row with unpublished changes stays with its holder until they publish it or change it back.
/// </summary>
public sealed record RowCheckoutState
{
    public required int SheetId { get; init; }

    public required int HolderUserId { get; init; }

    public required bool HasChanges { get; init; }
}
