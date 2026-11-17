namespace PUSpecSheet.Abstractions.Domain.Values;

public record TextValue : CellValue
{
    public required string? Value { get; init; }
}
