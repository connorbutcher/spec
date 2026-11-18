using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Linked;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Legacy.Values;

namespace PUSpecSheet.Api.DAL.Processors.Values.LinkedSpecSheetValue;

internal class AddLinkedSpecSheetValueOpHandler : OperationHandlerBase, IOperationHandler<AddLinkedSpecSheetValueOp>
{
    public AddLinkedSpecSheetValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(AddLinkedSpecSheetValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableCellId = ResolveId(operation.SpecSheetTableCellId, tempIdMap);

        var newLinkedSpecSheetValue = new DbLinkedSpecSheetValue
        {
            Id = 0,
            SpecSheetTableCellId = specSheetTableCellId,
            LinkedSpecSheetTypeId = operation.LinkedSpecSheetTypeId,
            LinkedSpecSheetPhaseId = operation.LinkedSpecSheetPhaseId,
            TableReferenceId = operation.TableReferenceId,
            LinkedReferenceId = operation.LinkedReferenceId
        };

        await DbContext.LinkedSpecSheetValues.AddAsync(newLinkedSpecSheetValue, ct);
    }

    public (int TempId, int RealId)? GetGeneratedId(AddLinkedSpecSheetValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(AddLinkedSpecSheetValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return IsTempId(operation.SpecSheetTableCellId) && !tempIdMap.ContainsKey(operation.SpecSheetTableCellId);
    }
}