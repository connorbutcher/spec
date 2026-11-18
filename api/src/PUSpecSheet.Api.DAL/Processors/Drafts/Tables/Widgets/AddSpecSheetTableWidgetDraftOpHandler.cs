using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database.Schema.Cell;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables.Widgets;

internal class AddSpecSheetTableWidgetDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddSpecSheetTableWidgetOp>
{
    public AddSpecSheetTableWidgetDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var tableId = ResolveId(operation.SpecSheetTableId, tempIdMap);
        var draft = await Tables.GetShownAsync(tableId, ct);

        var widget = DbContext.SheetTableWidgets.Local.FirstOrDefault(w => w.SheetTableId == tableId && w.TableTemplateWidgetId == operation.TableTemplateWidgetId)
            ?? await DbContext.SheetTableWidgets.FirstOrDefaultAsync(w => w.SheetTableId == tableId && w.TableTemplateWidgetId == operation.TableTemplateWidgetId, ct);
        if (widget is null)
        {
            widget = new DbSheetTableWidget
            {
                Id = 0,
                SheetTableId = tableId,
                TableTemplateWidgetId = operation.TableTemplateWidgetId,
                CreatedDateTime = DateTime.UtcNow
            };
            await DbContext.SheetTableWidgets.AddAsync(widget, ct);
        }

        var values = await Tables.LoadWidgetValuesAsync(draft, ct);
        var existing = values.FirstOrDefault(v => ReferenceEquals(v.SheetTableWidget, widget) || (widget.Id != 0 && v.SheetTableWidgetId == widget.Id));
        if (existing is not null)
        {
            existing.Value = operation.Value;
            return;
        }

        values.Add(new DbSheetTableWidgetValue
        {
            SheetTableRevisionId = 0,
            SheetTableWidgetId = widget.Id,
            Value = operation.Value,
            SheetTableWidget = widget
        });
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableId) && !tempIdMap.ContainsKey(operation.SpecSheetTableId);
    }
}