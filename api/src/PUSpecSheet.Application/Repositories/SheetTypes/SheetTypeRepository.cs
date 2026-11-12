using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Database.Entities.SheetTypes;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.SheetTypes;

public sealed class SheetTypeRepository : ISheetTypeRepository
{
    private readonly PuSpecSheetDbContext db;

    public SheetTypeRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<SheetTypeDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .AsNoTracking()
            .OrderBy(sheetType => sheetType.DisplayOrder)
            .ThenBy(sheetType => sheetType.Name)
            .Select(sheetType => new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetTypes.AnyAsync(sheetType => sheetType.Id == id, cancellationToken);
    }

    public async Task<bool> NameExistsAsync(string name, CancellationToken cancellationToken)
    {
        return await db.SheetTypes.AnyAsync(sheetType => sheetType.Name == name, cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(CancellationToken cancellationToken)
    {
        return await db.SheetTypes.MaxAsync(sheetType => (int?)sheetType.DisplayOrder, cancellationToken);
    }

    public async Task<List<int>> GetExistingIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .Where(sheetType => ids.Contains(sheetType.Id))
            .Select(sheetType => sheetType.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetIdByNameAsync(string name, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .Where(sheetType => sheetType.Name == name)
            .Select(sheetType => sheetType.Id)
            .SingleAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsByNamesAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .Where(sheetType => names.Contains(sheetType.Name))
            .Select(sheetType => sheetType.Id)
            .ToListAsync(cancellationToken);
    }

    public void Add(SheetType sheetType)
    {
        db.SheetTypes.Add(sheetType);
    }
}
