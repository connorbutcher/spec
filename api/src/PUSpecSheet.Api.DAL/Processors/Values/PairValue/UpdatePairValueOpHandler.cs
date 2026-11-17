using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Pair;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Values.PairValue;

internal class UpdatePairValueOpHandler : OperationHandlerBase, IOperationHandler<UpdatePairValueOp>
{
    public UpdatePairValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdatePairValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var pairValue = await DbContext.PairValues.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"PairValue with Id {operation.Id} not found.");

        if (operation.PairValueX is not null && operation.PairValueX.HasChanged)
        {
            pairValue.PairValueX = operation.PairValueX.Value;
        }

        if (operation.PairValueY is not null && operation.PairValueY.HasChanged)
        {
            pairValue.PairValueY = operation.PairValueY.Value;
        }
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
