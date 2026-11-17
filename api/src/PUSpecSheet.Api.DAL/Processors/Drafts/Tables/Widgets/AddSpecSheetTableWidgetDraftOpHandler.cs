using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

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

        draft.WidgetValues = DraftWidgetValues.Set(draft.WidgetValues, operation.TableTemplateWidgetId, operation.Value);
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