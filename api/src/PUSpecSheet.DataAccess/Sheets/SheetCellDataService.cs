using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Sheets.Layout;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class SheetCellDataService : ISheetCellDataService
{
    private readonly PuSpecSheetDbContext _db;

    public SheetCellDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<NewCell>> ListNewCellsAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        return await _db.SheetCells
            .AsNoTracking()
            .Where(cell => rowIds.Contains(cell.SheetRowId))
            .Select(cell => new NewCell
            {
                Id = cell.Id,
                SheetRowId = cell.SheetRowId,
                TemplateCellId = cell.TemplateCellId,
                SheetColumnBlockId = cell.SheetColumnBlockId,
                Kind = cell.TemplateCell.CellType.Kind,
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RequiredCell>> ListRequiredAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        // Only which cell, its row and its kind are needed, not the cells' templates and types.
        return await _db.SheetCells
            .Where(cell => rowIds.Contains(cell.SheetRowId) && cell.TemplateCell.IsRequired)
            .Select(cell => new RequiredCell { Id = cell.Id, SheetRowId = cell.SheetRowId, Kind = cell.TemplateCell.CellType.Kind })
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbSheetCell>> ListOnCurrentRowsAsync(int sheetId, CancellationToken cancellationToken)
    {
        var rowIds = _db.SheetRowRevisions
            .Where(revision => revision.SheetId == sheetId)
            .Current()
            .Where(revision => !revision.IsDeleted)
            .Select(revision => revision.SheetRowId);
        return await _db.SheetCells.AsNoTracking().Where(cell => rowIds.Contains(cell.SheetRowId)).ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<(int SheetRowId, int TemplateCellId, int? SheetColumnBlockId)>> ListMissingAsync(int tableId, CancellationToken cancellationToken)
    {
        var versionId = await _db.SheetTables
            .Where(table => table.Id == tableId)
            .Select(table => table.TableTemplateVersionId)
            .SingleAsync(cancellationToken);

        var rows = await _db.SheetRows
            .AsNoTracking()
            .Where(row => row.SheetSection.SheetTableId == tableId)
            .Select(row => new { row.Id, row.TemplateRowId })
            .ToListAsync(cancellationToken);
        var templateCells = await _db.TemplateCells
            .AsNoTracking()
            .Where(cell => cell.TemplateRow.TemplateSection.TableTemplateVersionId == versionId)
            .Select(cell => new { cell.Id, cell.TemplateRowId, cell.TemplateColumnBlockId })
            .ToListAsync(cancellationToken);
        var blocks = await _db.SheetColumnBlocks
            .AsNoTracking()
            .Where(block => block.SheetTableId == tableId)
            .Select(block => new { block.Id, block.TemplateColumnBlockId })
            .ToListAsync(cancellationToken);
        var existing = await _db.SheetCells
            .AsNoTracking()
            .Where(cell => cell.SheetRow.SheetSection.SheetTableId == tableId)
            .Select(cell => new { cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId })
            .ToListAsync(cancellationToken);

        return SheetCellPlanner.Missing(
            rows.Select(row => (row.Id, row.TemplateRowId)),
            templateCells.Select(cell => (cell.Id, cell.TemplateRowId, cell.TemplateColumnBlockId)),
            blocks.Select(block => (block.Id, block.TemplateColumnBlockId)),
            existing.Select(cell => (cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId)));
    }
}
