using PUSpecSheet.Abstractions.Domain.Common;

namespace PUSpecSheet.Abstractions.Operations.Values.Number;

public sealed record UpdateNumberValueOp : IChangeOp
{
    public required int Id { get; init; }

    public PropertyChanged<double?>? Value { get; init; }
}
