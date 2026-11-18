namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetPairValue : SheetCellValue
{
    public required string? ValueX { get; init; }

    public required string? ValueY { get; init; }
}
