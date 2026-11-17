namespace PUSpecSheet.Api.Database.Schema.Cell;

/// <summary>
/// A linked sheet cell's value in a row revision: the other sheet's type and phase, and the table and item in it, by ReferenceID.
/// </summary>
public record DbSheetLinkedSheetValue
{
    public required int SheetTableRowRevisionId { get; init; }

    public required int SheetTableCellId { get; init; }

    public required int? LinkedSpecSheetTypeId { get; init; }

    public required int? LinkedSpecSheetPhaseId { get; init; }

    public required Guid? TableReferenceId { get; init; }

    public required Guid? LinkedReferenceId { get; init; }
}
