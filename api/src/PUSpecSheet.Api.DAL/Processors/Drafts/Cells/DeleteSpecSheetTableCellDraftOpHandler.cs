using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Cells;

/// <summary>
/// Removes what the row's draft holds for the cell. The revisions keep a row's cells for as long as the row, so a cell goes
/// with its row; a cell in a row removed in the draft is left as it is.
/// </summary>
internal class DeleteSpecSheetTableCellDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<DeleteSpecSheetTableCellOp>
{
    public DeleteSpecSheetTableCellDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var rowId = await Rows.FindRowIdOfCellAsync(operation.Id, ct);
        var draft = await Rows.GetAsync(rowId, ct);
        if (draft.IsDeleted)
        {
            return;
        }

        Values.Clear(draft, operation.Id);
    }

    public (int TempId, int RealId)? GetGeneratedId(DeleteSpecSheetTableCellOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(DeleteSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}