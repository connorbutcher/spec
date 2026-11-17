using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Pair;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Values;

namespace PUSpecSheet.Api.DAL.Processors.Values.PairValue;

internal class AddPairValueOpHandler : OperationHandlerBase, IOperationHandler<AddPairValueOp>
{
    public AddPairValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddPairValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableCellId = ResolveId(operation.SpecSheetTableCellId, tempIdMap);

        var newPairValue = new DbPairValue
        {
            Id = 0,
            SpecSheetTableCellId = specSheetTableCellId,
            PairValueX = operation.ValueX,
            PairValueY = operation.ValueY,
        };

        await DbContext.PairValues.AddAsync(newPairValue, ct);
    }

    public (int TempId, int RealId)? GetGeneratedId(AddPairValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddPairValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableCellId) && !tempIdMap.ContainsKey(operation.SpecSheetTableCellId);
    }
}
