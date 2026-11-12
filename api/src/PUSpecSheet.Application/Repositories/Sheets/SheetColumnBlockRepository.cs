using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Database.Entities.Sheets;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Sheets;

public sealed class SheetColumnBlockRepository : ISheetColumnBlockRepository
{
    private readonly PuSpecSheetDbContext db;

    public SheetColumnBlockRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<SheetColumnBlock>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Where(block => block.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<SheetColumnBlock?> FindWithTemplateAndTableAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Include(block => block.TemplateColumnBlock)
            .Include(block => block.SheetTable)
            .SingleOrDefaultAsync(block => block.Id == id, cancellationToken);
    }

    public async Task<List<SheetColumnBlockLink>> ListLinksOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .AsNoTracking()
            .Where(block => block.SheetTableId == tableId)
            .Select(block => new SheetColumnBlockLink(block.Id, block.TemplateColumnBlockId))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlocks
            .Where(candidate => candidate.SheetTableId == tableId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public Task DeleteAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return SheetColumnBlockDeletion.DeleteAsync(db, ids, cancellationToken);
    }

    public void Add(SheetColumnBlock block)
    {
        db.SheetColumnBlocks.Add(block);
    }
}
