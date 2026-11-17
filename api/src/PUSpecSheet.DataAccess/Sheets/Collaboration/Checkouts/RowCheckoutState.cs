namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

public sealed record RowCheckoutState
{
    public required int SheetId { get; init; }

    public required int HolderUserId { get; init; }

    public required bool HasChanges { get; init; }
}
