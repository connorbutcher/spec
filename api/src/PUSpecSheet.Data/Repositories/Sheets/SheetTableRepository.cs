using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Repositories.Sheets;

public sealed class SheetTableRepository : ISheetTableRepository
{
    private readonly PuSpecSheetDbContext db;

    public SheetTableRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<SheetTable>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .AsNoTracking()
            .Include(table => table.TableTemplateVersion)
            .ThenInclude(version => version.TableTemplate)
            .Where(table => table.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<SheetTable?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetTables.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<SheetTable?> FindWithTemplateVersionAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .AsNoTracking()
            .Include(candidate => candidate.TableTemplateVersion)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .Where(table => table.Id == id)
            .Select(table => (int?)table.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .Where(candidate => candidate.SheetId == sheetId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await db.SheetTables.Where(table => table.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public void Add(SheetTable table)
    {
        db.SheetTables.Add(table);
    }
}
