using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Sheets.Values;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Sheets;

public sealed class SheetCellRepository : ISheetCellRepository
{
    private readonly PuSpecSheetDbContext db;

    public SheetCellRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<NewCell>> ListNewCellsAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        return await db.SheetCells
            .AsNoTracking()
            .Where(cell => rowIds.Contains(cell.SheetRowId))
            .Select(cell => new NewCell(cell.Id, cell.SheetRowId, cell.TemplateCellId, cell.SheetColumnBlockId, cell.TemplateCell.CellType.Kind))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RequiredCell>> ListRequiredAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        // Only which cell, its row and its kind are needed, not the cells' templates and types.
        return await db.SheetCells
            .Where(cell => rowIds.Contains(cell.SheetRowId) && cell.TemplateCell.IsRequired)
            .Select(cell => new RequiredCell(cell.Id, cell.SheetRowId, cell.TemplateCell.CellType.Kind))
            .ToListAsync(cancellationToken);
    }
}
