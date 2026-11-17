namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

/// <summary>A row one person is in, on one sheet, without having changed it yet.</summary>
public sealed record LiveRowCheckout
{
    public required int SheetId { get; init; }

    public required int RowId { get; init; }

    public required int UserId { get; init; }

    public required string DisplayName { get; init; }
}
