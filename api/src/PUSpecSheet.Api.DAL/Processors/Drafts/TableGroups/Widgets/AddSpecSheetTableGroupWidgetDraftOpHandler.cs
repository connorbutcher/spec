using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.TableGroups.Widgets;

internal class AddSpecSheetTableGroupWidgetDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddSpecSheetTableGroupWidgetOp>
{
    public AddSpecSheetTableGroupWidgetDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var groupId = ResolveId(operation.SpecSheetTableGroupId, tempIdMap);
        var draft = await Groups.GetShownAsync(groupId, ct);

        var widget = DbContext.SheetTableGroupWidgets.Local.FirstOrDefault(w => w.SheetTableGroupId == groupId && w.TableGroupTemplateWidgetId == operation.TableGroupTemplateWidgetId)
            ?? await DbContext.SheetTableGroupWidgets.FirstOrDefaultAsync(w => w.SheetTableGroupId == groupId && w.TableGroupTemplateWidgetId == operation.TableGroupTemplateWidgetId, ct);
        if (widget is null)
        {
            widget = new DbSheetTableGroupWidget
            {
                Id = 0,
                SheetTableGroupId = groupId,
                TableGroupTemplateWidgetId = operation.TableGroupTemplateWidgetId,
                CreatedDateTime = DateTime.UtcNow
            };
            await DbContext.SheetTableGroupWidgets.AddAsync(widget, ct);
        }

        var values = await Groups.LoadWidgetValuesAsync(draft, ct);
        var existing = values.FirstOrDefault(v => ReferenceEquals(v.SheetTableGroupWidget, widget) || (widget.Id != 0 && v.SheetTableGroupWidgetId == widget.Id));
        if (existing is not null)
        {
            existing.Value = operation.Value;
            return;
        }

        values.Add(new DbSheetTableGroupWidgetValue
        {
            SheetTableGroupRevisionId = 0,
            SheetTableGroupWidgetId = widget.Id,
            Value = operation.Value,
            SheetTableGroupWidget = widget
        });
    }

    public (int TempId, int RealId)? GetGeneratedId(AddSpecSheetTableGroupWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableGroupId) && !tempIdMap.ContainsKey(operation.SpecSheetTableGroupId);
    }
}