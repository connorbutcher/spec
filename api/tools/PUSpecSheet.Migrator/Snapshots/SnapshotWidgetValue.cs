using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// The value of one of a table's or group's widgets as a copy shows it. <see cref="TemplateWidgetId"/> is the
/// TableTemplateWidgetID or TableGroupTemplateWidgetID of the widget.
/// </summary>
public record SnapshotWidgetValue
{
    public required int TemplateWidgetId { get; init; }

    public required WidgetValue? Value { get; init; }
}
