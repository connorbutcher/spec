using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Values.Linked;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Values.LinkedSpecSheetValue;

internal class AddLinkedSpecSheetValueDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<AddLinkedSpecSheetValueOp>
{
    public AddLinkedSpecSheetValueDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(AddLinkedSpecSheetValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var cellId = ResolveId(operation.SpecSheetTableCellId, tempIdMap);
        var draft = await Rows.GetOfCellAsync(cellId, ct);

        Values.SetLinked(draft, cellId, new LinkedDraftValue
        {
            LinkedSpecSheetTypeId = operation.LinkedSpecSheetTypeId,
            LinkedSpecSheetPhaseId = operation.LinkedSpecSheetPhaseId,
            TableReferenceId = operation.TableReferenceId,
            LinkedReferenceId = operation.LinkedReferenceId
        });
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