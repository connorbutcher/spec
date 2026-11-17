using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Rows;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Rows;

internal class UpdateSpecSheetTableRowOpHandler : OperationHandlerBase, IOperationHandler<UpdateSpecSheetTableRowOp>
{
    public UpdateSpecSheetTableRowOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableRowOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableRow = await DbContext.SpecSheetTableRows.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"SpecSheetTableRow with Id {operation.Id} not found.");

        if (operation.RowNumber.HasValue)
        {
            specSheetTableRow.RowNumber = operation.RowNumber.Value;
        }

        specSheetTableRow.LastModifiedDateTime = DateTime.UtcNow;
        specSheetTableRow.LastModifiedByUserId = User.Id;
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