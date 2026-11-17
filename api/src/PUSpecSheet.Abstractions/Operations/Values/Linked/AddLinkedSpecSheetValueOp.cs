namespace PUSpecSheet.Abstractions.Operations.Values.Linked;

public sealed record AddLinkedSpecSheetValueOp : IChangeOp
{
    public required int SpecSheetTableCellId { get; init; }

    public int? LinkedSpecSheetTypeId { get; init; }

    public int? LinkedSpecSheetPhaseId { get; init; }

    public Guid? TableReferenceId { get; init; }

    public Guid? LinkedReferenceId { get; init; }
}
