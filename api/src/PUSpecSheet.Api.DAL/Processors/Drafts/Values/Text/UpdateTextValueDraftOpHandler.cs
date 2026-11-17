using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Text;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Values.Text;

internal class UpdateTextValueDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateTextValueOp>
{
    public UpdateTextValueDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateTextValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (operation.Value is null || !operation.Value.HasChanged)
        {
            return;
        }

        var draft = await GetRowDraftAsync(operation.Id, ct);
        Values.SetText(draft, operation.Id, operation.Value.Value);
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateTextValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateTextValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}