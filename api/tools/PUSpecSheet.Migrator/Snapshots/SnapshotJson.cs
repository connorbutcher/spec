using System.Text.Json;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;
using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// The JSON values of a sheet (configuration overrides, widget values and notes) written the same way every time, so
/// that equal values are equal text whether they were read from a copy or from a revision.
/// </summary>
internal static class SnapshotJson
{
    private static readonly JsonSerializerOptions _options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static string? Of(ITableProperties? properties)
    {
        return properties is null ? null : JsonSerializer.Serialize(properties, _options);
    }

    public static string? Of(ICellProperties? properties)
    {
        return properties is null ? null : JsonSerializer.Serialize(properties, _options);
    }

    public static string? Of(IReadOnlyList<SnapshotWidgetValue> widgetValues)
    {
        return widgetValues.Count == 0
            ? null
            : JsonSerializer.Serialize(widgetValues.OrderBy(widget => widget.TemplateWidgetId).ToList(), _options);
    }

    public static string? Of(IReadOnlyList<SnapshotNote> notes)
    {
        return notes.Count == 0 ? null : JsonSerializer.Serialize(notes, _options);
    }

    public static string Text(string? value)
    {
        return JsonSerializer.Serialize(value);
    }
}
