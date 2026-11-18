using System.Text.Json;
using PUSpecSheet.Abstractions.Domain.Sheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// What a table, group or row holds, as the revisions keep it, put in one string so that the item in two sheets can
/// be compared. Ids, and who made and changed the item, are left out.
/// </summary>
internal static class SheetCopySignatures
{
    public static string Of(SheetTable table)
    {
        return Json(new object?[] { table.Title, table.DisplayOrder, table.ConfigurationOverride, WidgetValuesOf(table), NotesOf(table) });
    }

    public static string Of(SheetTableGroup group)
    {
        return Json(WidgetValuesOf(group));
    }

    public static string Of(SheetTableRow row)
    {
        var cells = row.Cells
            .OrderBy(cell => cell.ReferenceId)
            .Select(cell => new object?[]
            {
                cell.ReferenceId,
                cell.Value with { SheetTableCellId = 0 },
                cell.ConfigurationOverride
            });

        return Json(new object?[] { row.DisplayOrder, cells });
    }

    private static IEnumerable<object?> WidgetValuesOf(SheetTable table)
    {
        return table.Widgets
            .OrderBy(widget => widget.TableTemplateWidgetId)
            .Select(widget => new object?[] { widget.TableTemplateWidgetId, widget.Value });
    }

    private static IEnumerable<object?> WidgetValuesOf(SheetTableGroup group)
    {
        return group.Widgets
            .OrderBy(widget => widget.TableGroupTemplateWidgetId)
            .Select(widget => new object?[] { widget.TableGroupTemplateWidgetId, widget.Value });
    }

    private static IEnumerable<object?> NotesOf(SheetTable table)
    {
        return table.Notes
            .OrderBy(note => note.CreatedDateTime)
            .ThenBy(note => note.CreatedByUserId)
            .ThenBy(note => note.SourceSystem, StringComparer.Ordinal)
            .ThenBy(note => note.Note, StringComparer.Ordinal)
            .Select(note => new object?[] { note.Note, note.SourceSystem, note.IsObsolete, note.CreatedByUserId, note.CreatedDateTime });
    }

    private static string Json(object? value)
    {
        return JsonSerializer.Serialize(value);
    }
}