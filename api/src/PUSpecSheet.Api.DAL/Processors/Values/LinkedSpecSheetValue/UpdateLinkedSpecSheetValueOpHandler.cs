using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Linked;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Values.LinkedSpecSheetValue;

internal class UpdateLinkedSpecSheetValueOpHandler : OperationHandlerBase, IOperationHandler<UpdateLinkedSpecSheetValueOp>
{
    public UpdateLinkedSpecSheetValueOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateLinkedSpecSheetValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var linkedSpecSheetValue = await DbContext.LinkedSpecSheetValues.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"LinkedSpecSheetValue with Id {operation.Id} not found.");

        if (operation.LinkedSpecSheetTypeId is not null && operation.LinkedSpecSheetTypeId.HasChanged)
        {
            linkedSpecSheetValue.LinkedSpecSheetTypeId = operation.LinkedSpecSheetTypeId.Value;
        }

        if (operation.LinkedSpecSheetPhaseId is not null && operation.LinkedSpecSheetPhaseId.HasChanged)
        {
            linkedSpecSheetValue.LinkedSpecSheetPhaseId = operation.LinkedSpecSheetPhaseId.Value;
        }

        if (operation.TableReferenceId is not null && operation.TableReferenceId.HasChanged)
        {
            linkedSpecSheetValue.TableReferenceId = operation.TableReferenceId.Value;
        }

        if (operation.LinkedReferenceId is not null && operation.LinkedReferenceId.HasChanged)
        {
            linkedSpecSheetValue.LinkedReferenceId = operation.LinkedReferenceId.Value;
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateLinkedSpecSheetValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateLinkedSpecSheetValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}