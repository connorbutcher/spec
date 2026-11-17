using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Tables;

internal class UpdateSpecSheetTableDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableOp>
{
    public UpdateSpecSheetTableDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var changesOverride = operation.ConfigurationOverride is not null && operation.ConfigurationOverride.HasChanged;
        if (operation.Title is null && !operation.DisplayOrder.HasValue && !changesOverride)
        {
            return;
        }

        var draft = await Tables.GetShownAsync(operation.Id, ct);

        if (operation.Title is not null)
        {
            draft.Title = operation.Title;
        }

        if (operation.DisplayOrder.HasValue)
        {
            draft.DisplayOrder = operation.DisplayOrder.Value;
        }

        if (changesOverride)
        {
            draft.ConfigurationOverride = operation.ConfigurationOverride!.Value;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}