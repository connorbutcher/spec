using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Rows;

internal class UpdateSpecSheetTableRowDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableRowOp>
{
    public UpdateSpecSheetTableRowDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (!operation.RowNumber.HasValue)
        {
            return;
        }

        var draft = await Rows.GetShownAsync(operation.Id, ct);
        draft.DisplayOrder = operation.RowNumber.Value;
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableRowOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}