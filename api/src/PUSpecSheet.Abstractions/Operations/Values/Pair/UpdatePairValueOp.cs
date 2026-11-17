using PUSpecSheet.Abstractions.Domain.Common;

namespace PUSpecSheet.Abstractions.Operations.Values.Pair;

public sealed record UpdatePairValueOp : IChangeOp
{
    public required int Id { get; init; }

    public PropertyChanged<string?>? PairValueX { get; init; }

    public PropertyChanged<string?>? PairValueY { get; init; }
}
