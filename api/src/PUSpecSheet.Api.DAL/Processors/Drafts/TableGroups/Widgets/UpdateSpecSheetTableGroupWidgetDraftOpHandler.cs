using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.TableGroups.Widgets;

internal class UpdateSpecSheetTableGroupWidgetDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableGroupWidgetOp>
{
    public UpdateSpecSheetTableGroupWidgetDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (operation.WidgetValue is null || !operation.WidgetValue.HasChanged)
        {
            return;
        }

        var widget = await Numbers.FindGroupWidgetAsync(operation.Id, ct);
        var draft = await Groups.GetShownAsync(widget.ItemId, ct);

        draft.WidgetValues = DraftWidgetValues.SetAt(draft.WidgetValues, widget.Index, operation.WidgetValue.Value);
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableGroupWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableGroupWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}