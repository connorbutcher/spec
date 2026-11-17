using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.Database.Schema.Cell;

/// <summary>
/// A pair cell's values in a row revision.
/// </summary>
public record DbSheetPairValue
{
    public required int SheetTableRowRevisionId { get; init; }

    public required int SheetTableCellId { get; init; }

    public required string? PairValueX { get; init; }

    public required string? PairValueY { get; init; }

    public virtual DbSheetTableCell? SheetTableCell { get; init; }
}
