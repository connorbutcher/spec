using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Abstractions.Operations.TableGroups;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Rows;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.TableGroups;

internal class DeleteSpecSheetTableGroupOpHandler : OperationHandlerBase, IOperationHandler<DeleteSpecSheetTableGroupOp>
{
    public DeleteSpecSheetTableGroupOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(DeleteSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableGroup = await DbContext.SpecSheetTableGroups
            .Include(g => g.Rows)
            .AsSplitQuery()
            .FirstOrDefaultAsync(g => g.Id == operation.Id, ct)
            ?? throw new InvalidOperationException($"SpecSheetTableGroup with Id {operation.Id} not found.");

        specSheetTableGroup.IsDeleted = true;

        foreach (var row in specSheetTableGroup.Rows)
        {
            var rowOpHandler = new DeleteSpecSheetTableRowOpHandler(DbContext, User);
            await rowOpHandler.ApplyAsync(new DeleteSpecSheetTableRowOp { Id = row.Id }, tempIdMap, ct);
        }

        var childGroups = await DbContext.SpecSheetTableGroups
            .Where(g => g.LinkedSpecSheetTableGroupId == specSheetTableGroup.Id)
            .ToListAsync(ct);
        foreach (var group in childGroups)
        {
            var groupOpHandler = new DeleteSpecSheetTableGroupOpHandler(DbContext, User);
            await groupOpHandler.ApplyAsync(new DeleteSpecSheetTableGroupOp { Id = group.Id }, tempIdMap, ct);
        }
    }

    public (int TempId, int RealId)? GetGeneratedId(DeleteSpecSheetTableGroupOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(DeleteSpecSheetTableGroupOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}
