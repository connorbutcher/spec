using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Layout;

// Adds the cells a table's rows are missing (see SheetCellPlanner). It runs inside a transaction holding an
// application lock for the table, so two requests that each add a row or a block at the same moment can't both insert
// the same cell or leave a gap.
public sealed class SheetCellFiller : ISheetCellFiller
{
    private readonly PuSpecSheetDbContext _db;
    private readonly ISheetTableDataService _sheetTableData;
    private readonly ISheetCellDataService _sheetCellData;

    public SheetCellFiller(PuSpecSheetDbContext db, ISheetTableDataService sheetTableData, ISheetCellDataService sheetCellData)
    {
        _db = db;
        _sheetTableData = sheetTableData;
        _sheetCellData = sheetCellData;
    }

    public async Task FillAsync(int tableId, CancellationToken cancellationToken)
    {
        var ownsTransaction = _db.Database.CurrentTransaction is null;
        var transaction = ownsTransaction
            ? await _db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            await _sheetTableData.LockForCellsAsync(tableId, cancellationToken);
            await AddMissingCellsAsync(tableId, cancellationToken);
            if (transaction is not null)
            {
                await transaction.CommitAsync(cancellationToken);
            }
        }
        finally
        {
            if (transaction is not null)
            {
                await transaction.DisposeAsync();
            }
        }
    }

    private async Task AddMissingCellsAsync(int tableId, CancellationToken cancellationToken)
    {
        var missing = await _sheetCellData.ListMissingAsync(tableId, cancellationToken);
        if (missing.Count > 0)
        {
            _db.SheetCells.AddRange(missing.Select(cell => new DbSheetCell
            {
                SheetRowId = cell.SheetRowId,
                TemplateCellId = cell.TemplateCellId,
                SheetColumnBlockId = cell.SheetColumnBlockId,
            }));
            await _db.SaveSheetChangesAsync(cancellationToken);
        }
    }
}
