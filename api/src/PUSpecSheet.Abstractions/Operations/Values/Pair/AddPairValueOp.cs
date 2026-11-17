namespace PUSpecSheet.Abstractions.Operations.Values.Pair;

public sealed record AddPairValueOp : IChangeOp
{
    public required int SpecSheetTableCellId { get; init; }

    public string? ValueX { get; init; }

    public string? ValueY { get; init; }
}
