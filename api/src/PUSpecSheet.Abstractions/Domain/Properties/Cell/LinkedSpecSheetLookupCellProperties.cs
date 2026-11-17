namespace PUSpecSheet.Abstractions.Domain.Properties.Cell;

public sealed class LinkedSpecSheetLookupCellProperties : ICellProperties
{
    public required int? LinkedSpecSheetTypeId { get; init; }

    public required int? LinkedSpecSheetPhaseId { get; init; }

    public required Guid? TableReferenceId { get; init; }

    public required Guid? RowReferenceId { get; init; }

    public required Guid? TableCellTypeKey { get; init; }

    public required bool LockToLatestPublished { get; init; } = false;
}
