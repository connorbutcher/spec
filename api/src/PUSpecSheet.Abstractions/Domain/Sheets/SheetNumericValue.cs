namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetNumericValue : SheetCellValue
{
    public required double? Value { get; init; }
}
