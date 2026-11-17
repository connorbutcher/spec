namespace PUSpecSheet.Abstractions.Domain.Properties.Cell;

public sealed class PairCellProperties : ICellProperties
{
    public required string? DefaultValueX { get; init; }

    public required string? DefaultValueY { get; init; }
}
