using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database;

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
}
