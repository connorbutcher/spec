namespace PUSpecSheet.Query.Models.Values;

public record SlimPairValue : SlimValue
{
    public required string? ValueX { get; init; }

    public required string? ValueY { get; init; }
}
