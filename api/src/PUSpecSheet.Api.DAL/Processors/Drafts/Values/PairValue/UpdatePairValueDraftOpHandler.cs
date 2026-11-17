using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Pair;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Values.PairValue;

internal class UpdatePairValueDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdatePairValueOp>
{
    public UpdatePairValueDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdatePairValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var changesX = operation.PairValueX is not null && operation.PairValueX.HasChanged;
        var changesY = operation.PairValueY is not null && operation.PairValueY.HasChanged;
        if (!changesX && !changesY)
        {
            return;
        }

        var draft = await GetRowDraftAsync(operation.Id, ct);
        var existing = Values.PairOf(draft, operation.Id);
        var valueX = changesX ? operation.PairValueX!.Value : existing?.PairValueX;
        var valueY = changesY ? operation.PairValueY!.Value : existing?.PairValueY;

        Values.SetPair(draft, operation.Id, valueX, valueY);
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdatePairValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdatePairValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}