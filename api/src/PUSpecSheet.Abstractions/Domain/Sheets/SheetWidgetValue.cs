using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// The value of one of a table's or group's widgets, held on its revision. <see cref="TemplateWidgetId"/> is the
/// TableTemplateWidgetID or TableGroupTemplateWidgetID of the widget.
/// </summary>
public record SheetWidgetValue
{
    public required int TemplateWidgetId { get; init; }

    public required WidgetValue? Value { get; init; }
}
