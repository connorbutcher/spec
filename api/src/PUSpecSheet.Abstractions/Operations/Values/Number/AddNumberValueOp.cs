namespace PUSpecSheet.Abstractions.Operations.Values.Number;

public sealed record AddNumberValueOp : IChangeOp
{
    public required int SpecSheetTableCellId { get; init; }

    public double? Value { get; init; }
}
