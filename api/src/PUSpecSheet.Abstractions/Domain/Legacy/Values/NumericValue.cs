namespace PUSpecSheet.Abstractions.Domain.Legacy.Values;

public record NumericValue : CellValue
{
    public required double? Value { get; set; }
}
