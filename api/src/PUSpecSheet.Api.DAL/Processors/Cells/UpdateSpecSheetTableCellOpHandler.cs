using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Cells;

internal class UpdateSpecSheetTableCellOpHandler : OperationHandlerBase, IOperationHandler<UpdateSpecSheetTableCellOp>
{
    public UpdateSpecSheetTableCellOpHandler(PUSpecSheetDbContext dbContext, User user)
        : base(dbContext, user)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        var specSheetTableCell = await DbContext.SpecSheetTableCells.FindAsync([operation.Id], ct)
            ?? throw new InvalidOperationException($"SpecSheetTableCell with Id {operation.Id} not found.");

        if (operation.RowNumber.HasValue)
        {
            specSheetTableCell.RowNumber = operation.RowNumber.Value;
        }

        if (operation.ColumnNumber.HasValue)
        {
            specSheetTableCell.ColumnNumber = operation.ColumnNumber.Value;
        }

        if (operation.ConfigurationOverride is not null && operation.ConfigurationOverride.HasChanged)
        {
            specSheetTableCell.ConfigurationOverride = operation.ConfigurationOverride.Value;
        }

        specSheetTableCell.LastModifiedDateTime = DateTime.UtcNow;
        specSheetTableCell.LastModifiedByUserId = User.Id;
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableCellOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }
}