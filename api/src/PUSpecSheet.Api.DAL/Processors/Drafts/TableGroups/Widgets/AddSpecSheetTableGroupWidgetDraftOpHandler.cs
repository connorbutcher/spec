using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups.Widgets;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

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

        draft.WidgetValues = DraftWidgetValues.Set(draft.WidgetValues, operation.TableGroupTemplateWidgetId, operation.Value);
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