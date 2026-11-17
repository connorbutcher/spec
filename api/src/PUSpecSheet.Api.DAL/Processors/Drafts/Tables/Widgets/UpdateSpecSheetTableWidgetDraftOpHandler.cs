using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables.Widgets;

internal class UpdateSpecSheetTableWidgetDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableWidgetOp>
{
    public UpdateSpecSheetTableWidgetDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (operation.WidgetValue is null || !operation.WidgetValue.HasChanged)
        {
            return;
        }

        var widget = await Numbers.FindTableWidgetAsync(operation.Id, ct);
        var draft = await Tables.GetShownAsync(widget.ItemId, ct);

        draft.WidgetValues = DraftWidgetValues.SetAt(draft.WidgetValues, widget.Index, operation.WidgetValue.Value);
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableWidgetOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableWidgetOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}