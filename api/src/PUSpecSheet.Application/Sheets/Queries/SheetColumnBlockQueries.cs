using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The column blocks of horizontal sheet tables.</summary>
public static class SheetColumnBlockQueries
{
    /// <summary>The column blocks of the sheet's tables with the template column block each copies, read only.</summary>
    public static async Task<List<DbSheetColumnBlock>> ListOnSheetAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Where(block => block.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The column block with its template column block and table, read only; null when there is no such block.</summary>
    public static async Task<DbSheetColumnBlock?> FindWithTemplateAndTableAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Include(block => block.SheetTable)
            .SingleOrDefaultAsync(block => block.Id == id, cancellationToken);
    }

    /// <summary>The table's column blocks and the template column block of each.</summary>
    public static async Task<List<SheetColumnBlockLink>> ListLinksOnTableAsync(PuSpecSheetDbContext db, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Where(block => block.SheetTableId == tableId)
            .Select(block => new SheetColumnBlockLink(block.Id, block.TemplateColumnBlockId))
            .ToListAsync(cancellationToken);
    }

    /// <summary>The ids of the table's column blocks.</summary>
    public static async Task<List<int>> ListIdsOnTableAsync(PuSpecSheetDbContext db, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .Where(candidate => candidate.SheetTableId == tableId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Deletes column blocks nobody but their author ever saw, with their cells and any values typed into
    /// those cells, straight in the database.
    /// </summary>
    public static Task DeleteAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return SheetColumnBlockDeletion.DeleteAsync(db, ids, cancellationToken);
    }
}
