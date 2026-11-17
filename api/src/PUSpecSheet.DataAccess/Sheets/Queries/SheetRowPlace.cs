namespace PUSpecSheet.DataAccess.Sheets.Queries;

public sealed record SheetRowPlace
{
    public required int SheetSectionId { get; init; }

    public required int SheetTableId { get; init; }
}
