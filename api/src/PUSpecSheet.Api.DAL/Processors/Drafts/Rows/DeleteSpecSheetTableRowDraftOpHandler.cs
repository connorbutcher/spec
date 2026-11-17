using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Rows;

internal class DeleteSpecSheetTableRowDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<DeleteSpecSheetTableRowOp>
{
    public DeleteSpecSheetTableRowDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var draft = await Rows.GetAsync(operation.Id, ct);
        draft.IsDeleted = true;
    }

    public (int TempId, int RealId)? GetGeneratedId(DeleteSpecSheetTableRowOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(DeleteSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}