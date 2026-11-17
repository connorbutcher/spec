namespace PUSpecSheet.Abstractions.Domain.Values;

public record PairValue : CellValue
{
    public required string? ValueX { get; init; }

    public required string? ValueY { get; init; }
}
