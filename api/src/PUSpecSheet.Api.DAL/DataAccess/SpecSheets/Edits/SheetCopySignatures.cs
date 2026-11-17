using System.Text.Json;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Domain.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// What a table, group or row holds, as the revisions keep it, put in one string so that the item in two spec sheets can
/// be compared. Ids, and who made and changed the item, are left out.
/// </summary>
internal static class SheetCopySignatures
{
    public static string Of(SpecSheetTable table)
    {
        return Json(new object?[] { table.Title, table.DisplayOrder, table.ConfigurationOverride, WidgetValuesOf(table), NotesOf(table) });
    }

    public static string Of(SpecSheetTableGroup group)
    {
        return Json(WidgetValuesOf(group));
    }

    public static string Of(SpecSheetTableRow row)
    {
        var cells = row.Cells
            .OrderBy(cell => cell.ReferenceId)
            .Select(cell => new object?[]
            {
                cell.ReferenceId,
                cell.CellValue is null ? null : cell.CellValue with { Id = 0, SpecSheetTableCellId = 0 },
                cell.ConfigurationOverride
            });

        return Json(new object?[] { row.RowNumber, cells });
    }

    public static ICollection<SheetWidgetValue>? WidgetValuesOf(SpecSheetTable table)
    {
        return table.Widgets.Count == 0
            ? null
            : [.. table.Widgets.Select(widget => new SheetWidgetValue { TemplateWidgetId = widget.TableTemplateWidgetId, Value = widget.Value.Value })];
    }

    public static ICollection<SheetWidgetValue>? WidgetValuesOf(SpecSheetTableGroup group)
    {
        return group.Widgets.Count == 0
            ? null
            : [.. group.Widgets.Select(widget => new SheetWidgetValue { TemplateWidgetId = widget.TableGroupTemplateWidgetId, Value = widget.Value.Value })];
    }

    public static ICollection<SheetTableNote>? NotesOf(SpecSheetTable table)
    {
        return table.Notes.Count == 0
            ? null
            : [.. table.Notes.Select(note => new SheetTableNote
            {
                Note = note.Note,
                SourceSystem = note.SourceSystem,
                IsObsolete = note.IsObsolete,
                CreatedByUserId = note.CreatedByUserId,
                CreatedDateTime = note.CreatedDateTime
            })];
    }

    private static string Json(object? value)
    {
        return JsonSerializer.Serialize(value);
    }
}