using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Adds the cells a table's rows are missing (see <see cref="SheetCellPlanner"/>). It runs inside a
/// transaction holding an application lock for the table, so two requests that each add a row or a
/// block at the same moment can't both insert the same cell or leave a gap.
/// </summary>
public sealed class SheetCellFiller(PuSpecSheetDbContext db) : ISheetCellFiller
{
    private const int LockTimeoutMilliseconds = 15000;

    public async Task FillAsync(int tableId, CancellationToken cancellationToken)
    {
        var ownsTransaction = db.Database.CurrentTransaction is null;
        var transaction = ownsTransaction
            ? await db.Database.BeginTransactionAsync(cancellationToken)
            : null;

        try
        {
            await TakeTableLockAsync(tableId, cancellationToken);

            var versionId = await db.SheetTables
                .Where(table => table.Id == tableId)
                .Select(table => table.TableTemplateVersionId)
                .SingleAsync(cancellationToken);

            var rows = await db.SheetRows
                .AsNoTracking()
                .Where(row => row.SheetSection.SheetTableId == tableId)
                .Select(row => new { row.Id, row.TemplateRowId })
                .ToListAsync(cancellationToken);
            var templateCells = await db.TemplateCells
                .AsNoTracking()
                .Where(cell => cell.TemplateRow.TemplateSection.TableTemplateVersionId == versionId)
                .Select(cell => new { cell.Id, cell.TemplateRowId, cell.TemplateColumnBlockId })
                .ToListAsync(cancellationToken);
            var blocks = await db.SheetColumnBlocks
                .AsNoTracking()
                .Where(block => block.SheetTableId == tableId)
                .Select(block => new { block.Id, block.TemplateColumnBlockId })
                .ToListAsync(cancellationToken);
            var existing = await db.SheetCells
                .AsNoTracking()
                .Where(cell => cell.SheetRow.SheetSection.SheetTableId == tableId)
                .Select(cell => new { cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId })
                .ToListAsync(cancellationToken);

            var missing = SheetCellPlanner.Missing(
                rows.Select(row => (row.Id, row.TemplateRowId)),
                templateCells.Select(cell => (cell.Id, cell.TemplateRowId, cell.TemplateColumnBlockId)),
                blocks.Select(block => (block.Id, block.TemplateColumnBlockId)),
                existing.Select(cell => (cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId)));

            if (missing.Count > 0)
            {
                db.SheetCells.AddRange(missing.Select(cell => new SheetCell
                {
                    SheetRowId = cell.SheetRowId,
                    TemplateCellId = cell.TemplateCellId,
                    SheetColumnBlockId = cell.SheetColumnBlockId,
                }));
                await db.SaveSheetChangesAsync(cancellationToken);
            }

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

    /// <summary>Waits for any other request filling the same table; released when the transaction ends.</summary>
    private async Task TakeTableLockAsync(int tableId, CancellationToken cancellationToken)
    {
        var resource = $"SheetCells:{tableId}";
        await db.Database.ExecuteSqlAsync(
            $"""
            DECLARE @result int;
            EXEC @result = sp_getapplock
                @Resource = {resource},
                @LockMode = 'Exclusive',
                @LockOwner = 'Transaction',
                @LockTimeout = {LockTimeoutMilliseconds};
            IF @result < 0 THROW 50000, 'Another change to this table is taking too long. Try again.', 1;
            """,
            cancellationToken);
    }
}
