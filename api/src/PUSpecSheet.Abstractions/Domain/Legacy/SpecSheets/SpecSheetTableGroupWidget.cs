using PUSpecSheet.Abstractions.Domain.Legacy.Values;

namespace PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;

public record SpecSheetTableGroupWidget
{
    public required int Id { get; init; }

    public required int SpecSheetTableGroupId { get; init; }

    public required int TableGroupTemplateWidgetId { get; init; }

    public required TableGroupWidgetValue Value { get; set; }
}
