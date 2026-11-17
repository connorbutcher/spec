using PUSpecSheet.Abstractions.Domain.Common;

namespace PUSpecSheet.Abstractions.Operations.TableGroups;

public sealed record UpdateSpecSheetTableGroupOp : IChangeOp
{
    public required int Id { get; init; }

    public int? Order { get; init; }

    public PropertyChanged<int?>? LinkedSpecSheetTableGroupId { get; init; }
}
