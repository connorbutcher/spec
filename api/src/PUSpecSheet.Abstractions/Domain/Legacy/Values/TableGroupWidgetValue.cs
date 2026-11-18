using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Domain.Legacy.Values;

public record TableGroupWidgetValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableGroupWidgetId { get; init; }

    public required WidgetValue? Value { get; set; }
}
