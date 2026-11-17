using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Pair;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Values.PairValue;

internal class AddPairValueDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddPairValueOp>
{
    public AddPairValueDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddPairValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var cellId = ResolveId(operation.SpecSheetTableCellId, tempIdMap);
        var draft = await Rows.GetOfCellAsync(cellId, ct);

        Values.SetPair(draft, cellId, operation.ValueX, operation.ValueY);
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