using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Operations.Cells;
using PUSpecSheet.Api.DAL.Processors.Base;
using PUSpecSheet.Api.DAL.Processors.Drafts.Base;
using PUSpecSheet.Api.Database;

namespace PUSpecSheet.Api.DAL.Processors.Drafts.Cells;

/// <summary>
/// Changes a cell's configuration override. A cell's row number is its row's and its column is its template cell's, so
/// neither is changed here, and moving a cell to another column is refused.
/// </summary>
internal class UpdateSpecSheetTableCellDraftOpHandler : DraftOperationHandlerBase, IOperationHandler<UpdateSpecSheetTableCellOp>
{
    public UpdateSpecSheetTableCellDraftOpHandler(PUSpecSheetDbContext dbContext, User user, int sheetId)
        : base(dbContext, user, sheetId)
    {
    }

    public async Task ApplyAsync(UpdateSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap, CancellationToken ct)
    {
        if (operation.ColumnNumber.HasValue)
        {
            await EnsureColumnAsync(operation.Id, operation.ColumnNumber.Value, ct);
        }

        if (operation.ConfigurationOverride is null || !operation.ConfigurationOverride.HasChanged)
        {
            return;
        }

        var draft = await Rows.GetOfCellAsync(operation.Id, ct);
        Values.SetConfigurationOverride(draft, operation.Id, operation.ConfigurationOverride.Value);
    }

    public (int TempId, int RealId)? GetGeneratedId(UpdateSpecSheetTableCellOp operation)
    {
        return null;
    }

    public bool HasUnresolvedDependencies(UpdateSpecSheetTableCellOp operation, IReadOnlyDictionary<int, int> tempIdMap)
    {
        return false;
    }

    private async Task EnsureColumnAsync(int cellId, int columnNumber, CancellationToken ct)
    {
        var templateColumnNumber = await DbContext.SheetTableCells
            .Where(c => c.Id == cellId)
            .Select(c => (int?)c.TableCellTemplate!.ColumnNumber)
            .FirstOrDefaultAsync(ct)
            ?? throw new KeyNotFoundException($"SheetTableCell with Id {cellId} not found.");

        if (templateColumnNumber != columnNumber)
        {
            throw new InvalidOperationException($"SheetTableCell with Id {cellId} is in its template cell's column {templateColumnNumber}; it can't be moved to column {columnNumber}.");
        }
    }
}