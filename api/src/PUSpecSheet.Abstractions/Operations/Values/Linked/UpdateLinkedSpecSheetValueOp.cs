using PUSpecSheet.Abstractions.Domain.Common;

namespace PUSpecSheet.Abstractions.Operations.Values.Linked;

public sealed record UpdateLinkedSpecSheetValueOp : IChangeOp
{
    public required int Id { get; init; }

    public PropertyChanged<int?>? LinkedSpecSheetTypeId { get; init; }

    public PropertyChanged<int?>? LinkedSpecSheetPhaseId { get; init; }

    public PropertyChanged<Guid?>? TableReferenceId { get; init; }

    public PropertyChanged<Guid?>? LinkedReferenceId { get; init; }
}
