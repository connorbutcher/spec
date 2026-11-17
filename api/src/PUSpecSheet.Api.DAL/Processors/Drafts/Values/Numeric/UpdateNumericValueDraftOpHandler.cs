using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Number;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Values.Numeric;

internal class UpdateNumericValueDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateNumberValueOp>
{
    public UpdateNumericValueDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateNumberValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (operation.Value is null || !operation.Value.HasChanged)
        {
            return;
        }

        var draft = await GetRowDraftAsync(operation.Id, ct);
        Values.SetNumber(draft, operation.Id, operation.Value.Value);
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateNumberValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateNumberValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}