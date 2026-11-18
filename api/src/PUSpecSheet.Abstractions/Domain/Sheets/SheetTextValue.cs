namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetTextValue : SheetCellValue
{
    public required string? Value { get; init; }
}
