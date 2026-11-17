using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.TableGroups;

internal class UpdateSpecSheetTableGroupDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableGroupOp>
{
    public UpdateSpecSheetTableGroupDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var changesLink = operation.LinkedSpecSheetTableGroupId is not null && operation.LinkedSpecSheetTableGroupId.HasChanged;
        if (!operation.Order.HasValue && !changesLink)
        {
            return;
        }

        var draft = await Groups.GetShownAsync(operation.Id, ct);

        if (operation.Order.HasValue)
        {
            draft.DisplayOrder = operation.Order.Value;
        }

        if (changesLink)
        {
            var linkedGroupId = operation.LinkedSpecSheetTableGroupId!.Value;
            draft.LinkedSheetTableGroupReferenceId = linkedGroupId.HasValue
                ? await Groups.FindReferenceIdAsync(linkedGroupId.Value, ct)
                : null;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableGroupOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}