using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Domain.Legacy.Values;

public record TableWidgetValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableWidgetId { get; init; }

    public required WidgetValue? Value { get; set; }
}
