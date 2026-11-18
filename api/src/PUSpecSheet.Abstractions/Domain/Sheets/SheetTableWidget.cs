using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A widget of a table and its value in the table revision that is shown.
/// </summary>
public record SheetTableWidget
{
    public required int Id { get; init; }

    public required int SheetTableId { get; init; }

    public required int TableTemplateWidgetId { get; init; }

    public required WidgetValue? Value { get; init; }
}
