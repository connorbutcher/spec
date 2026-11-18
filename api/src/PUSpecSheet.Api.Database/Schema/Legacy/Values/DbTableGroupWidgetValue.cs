using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Api.Database.Schema.Legacy.Values;

public record DbTableGroupWidgetValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableGroupWidgetId { get; init; }

    public required WidgetValue? Value { get; set; }
}
