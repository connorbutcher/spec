namespace PUSpecSheet.Api.Database.Schema.Cell;

/// <summary>
/// A text cell's value in a row revision.
/// </summary>
public record DbSheetTextValue
{
    public required int SheetTableRowRevisionId { get; init; }

    public required int SheetTableCellId { get; init; }

    public required string Value { get; init; }
}
