using PUSpecSheet.Abstractions.Domain.Legacy.Values;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

public record SpecSheetTableWidget
{
    public required int Id { get; init; }

    public required int SpecSheetTableId { get; init; }

    public required int TableTemplateWidgetId { get; init; }

    public required TableWidgetValue Value { get; set; }
}
