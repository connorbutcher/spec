namespace PUSpecSheet.Abstractions.Domain.Values;

public record NumericValue : CellValue
{
    public required double? Value { get; set; }
}
