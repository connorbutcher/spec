using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Api.Database.Schema.Values;

public record DbTableWidgetValue
{
    public required int Id { get; init; }

    public required int SpecSheetTableWidgetId { get; init; }

    public required WidgetValue? Value { get; set; }
}
