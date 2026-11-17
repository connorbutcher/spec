using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Cells;

internal class DeleteSpecSheetTableCellOpHandler : OperationHandlerBase, IOperationHandler<DeleteSpecSheetTableCellOp>
{
    public DeleteSpecSheetTableCellOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableCell = await DbContext.SpecSheetTableCells.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"SpecSheetTableCells with Id {operation.Id} not found.");

        specSheetTableCell.IsDeleted = true;
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
