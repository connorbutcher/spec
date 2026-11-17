using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Abstractions.Operations.Tables;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.TableGroups;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Tables;

internal class DeleteSpecSheetTableOpHandler : OperationHandlerBase, IOperationHandler<DeleteSpecSheetTableOp>
{
    public DeleteSpecSheetTableOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTable = await DbContext.SpecSheetTables
            .Include(t => t.Groups)
                .ThenInclude(g => g.Rows)
                    .ThenInclude(r => r.Cells)
            .AsSplitQuery()
            .FirstOrDefaultAsync(t => t.Id == operation.Id, ct)
            ?? throw new InvalidOperationException($"SpecSheetTable with Id {operation.Id} not found.");

        specSheetTable.IsDeleted = true;

        foreach (var group in specSheetTable.Groups)
        {
            var groupOpHandler = new DeleteSpecSheetTableGroupOpHandler(DbContext, User);
            await groupOpHandler.ApplyAsync(new DeleteSpecSheetTableGroupOp { Id = group.Id }, tempIdMap, ct);
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(DeleteSpecSheetTableOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(DeleteSpecSheetTableOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}
