using Microsoft.EntityFrameworkCore;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Sheets.Queries;

/// <summary>The cells of sheet rows.</summary>
public static class SheetCellQueries
{
    /// <summary>The cells of the rows, with what each copies and its kind.</summary>
    public static async Task<List<NewCell>> ListNewCellsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        return await db.SheetCells
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

    /// <summary>The cells of the rows that must be filled in.</summary>
    public static async Task<List<RequiredCell>> ListRequiredAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        // Only which cell, its row and its kind are needed, not the cells' templates and types.
        return await db.SheetCells
            .Where(cell => rowIds.Contains(cell.SheetRowId) && cell.TemplateCell.IsRequired)
            .Select(cell => new RequiredCell { Id = cell.Id, SheetRowId = cell.SheetRowId, Kind = cell.TemplateCell.CellType.Kind })
            .ToListAsync(cancellationToken);
    }
}
