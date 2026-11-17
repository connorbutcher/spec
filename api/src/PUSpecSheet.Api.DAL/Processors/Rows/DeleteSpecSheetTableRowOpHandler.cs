using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Cells;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Rows;

internal class DeleteSpecSheetTableRowOpHandler : OperationHandlerBase, IOperationHandler<DeleteSpecSheetTableRowOp>
{
    public DeleteSpecSheetTableRowOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableRow = await DbContext.SpecSheetTableRows
            .Include(r => r.Cells)
            .AsSplitQuery()
            .FirstOrDefaultAsync(r => r.Id == operation.Id, ct)
            ?? throw new InvalidOperationException($"SpecSheetTableRows with Id {operation.Id} not found.");

        specSheetTableRow.IsDeleted = true;

        foreach (var cell in specSheetTableRow.Cells)
        {
            var cellOpHandler = new DeleteSpecSheetTableCellOpHandler(DbContext, User);
            await cellOpHandler.ApplyAsync(new DeleteSpecSheetTableCellOp { Id = cell.Id }, tempIdMap, ct);
        }
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
