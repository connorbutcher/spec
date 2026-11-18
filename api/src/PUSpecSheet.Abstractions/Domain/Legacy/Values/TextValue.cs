namespace PUSpecSheet.Abstractions.Domain.Legacy.Values;

public record TextValue : CellValue
{
    public required string? Value { get; init; }
}
