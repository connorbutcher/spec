using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.Values.WidgetValues;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Base;

/// <summary>
/// Changes the widget values a table or group draft holds. Each change gives a new list, so the draft is seen to change.
/// </summary>
internal static class DraftWidgetValues
{
    /// <summary>
    /// The values with the widget's value set: replacing the value its template widget has, or added after the others.
    /// </summary>
    public static ICollection<SheetWidgetValue> Set(ICollection<SheetWidgetValue>? values, int templateWidgetId, WidgetValue? value)
    {
        var updated = values is null ? [] : values.ToList();
        var index = updated.FindIndex(widget => widget.TemplateWidgetId == templateWidgetId);
        var widget = new SheetWidgetValue { TemplateWidgetId = templateWidgetId, Value = value };
        if (index < 0)
        {
            updated.Add(widget);
        }
        else
        {
            updated[index] = widget;
        }

        return updated;
    }

    /// <summary>
    /// The values with the value of the widget at the index changed.
    /// </summary>
    public static ICollection<SheetWidgetValue> SetAt(ICollection<SheetWidgetValue>? values, int index, WidgetValue? value)
    {
        var updated = values is null ? [] : values.ToList();
        if (index >= updated.Count)
        {
            throw new InvalidOperationException($"The draft holds no widget at position {index + 1}.");
        }

        updated[index] = updated[index] with { Value = value };
        return updated;
    }
}