namespace PUSpecSheet.Api.Database.Schema.Cell;

/// <summary>
/// A numeric cell's value in a row revision.
/// </summary>
public record DbSheetNumericValue
{
    public required int SheetTableRowRevisionId { get; init; }

    public required int SheetTableCellId { get; init; }

    public required double Value { get; init; }
}
