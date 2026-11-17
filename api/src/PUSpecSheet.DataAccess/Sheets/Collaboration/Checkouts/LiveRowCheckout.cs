namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

public sealed record LiveRowCheckout
{
    public required int SheetId { get; init; }

    public required int RowId { get; init; }

    public required int UserId { get; init; }

    public required string DisplayName { get; init; }
}
