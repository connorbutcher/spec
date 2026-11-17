namespace PUSpecSheet.Abstractions.Domain.Styling;

public record CellStyling
{
    public required string BackgroundColor { get; init; }

    public required int FontWeight { get; init; }

    public required string FontSize { get; init; }

    public int? Width { get; init; }

    public required bool IsSticky { get; init; }

    public static CellStyling Default => new()
    {
        BackgroundColor = "#FFFFFF",
        FontWeight = 400,
        FontSize = "inherit",
        IsSticky = false,
        Width = null
    };
}
