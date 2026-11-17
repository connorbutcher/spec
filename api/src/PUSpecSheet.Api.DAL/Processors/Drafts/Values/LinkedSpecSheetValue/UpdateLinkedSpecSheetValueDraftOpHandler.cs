using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Common;
using PUSpecSheet.Abstractions.Operations.Values.Linked;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Values.LinkedSpecSheetValue;

internal class UpdateLinkedSpecSheetValueDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateLinkedSpecSheetValueOp>
{
    public UpdateLinkedSpecSheetValueDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateLinkedSpecSheetValueOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (!HasChanged(operation.LinkedSpecSheetTypeId)
            && !HasChanged(operation.LinkedSpecSheetPhaseId)
            && !HasChanged(operation.TableReferenceId)
            && !HasChanged(operation.LinkedReferenceId))
        {
            return;
        }

        var draft = await Rows.GetOfCellAsync(operation.Id, ct);
        var existing = Values.LinkedOf(draft, operation.Id);

        Values.SetLinked(draft, operation.Id, new LinkedDraftValue
        {
            LinkedSpecSheetTypeId = Changed(operation.LinkedSpecSheetTypeId, existing?.LinkedSpecSheetTypeId),
            LinkedSpecSheetPhaseId = Changed(operation.LinkedSpecSheetPhaseId, existing?.LinkedSpecSheetPhaseId),
            TableReferenceId = Changed(operation.TableReferenceId, existing?.TableReferenceId),
            LinkedReferenceId = Changed(operation.LinkedReferenceId, existing?.LinkedReferenceId)
        });
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateLinkedSpecSheetValueOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateLinkedSpecSheetValueOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }

    private static bool HasChanged<T>(PropertyChanged<T>? change)
    {
        return change is not null && change.HasChanged;
    }

    private static T? Changed<T>(PropertyChanged<T?>? change, T? current)
        where T : struct
    {
        return change is not null && change.HasChanged ? change.Value : current;
    }
}