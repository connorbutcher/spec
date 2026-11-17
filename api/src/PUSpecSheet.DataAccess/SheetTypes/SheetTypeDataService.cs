using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Mappers.SheetTypes;
using PUSpecSheet.Database.Models.SheetTypes;

namespace PUSpecSheet.DataAccess.SheetTypes;

public sealed class SheetTypeDataService : ISheetTypeDataService
{
    private readonly PuSpecSheetDbContext db;

    public SheetTypeDataService(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<SheetTypeDto>> ListAsync(CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .AsNoTracking()
            .OrderBy(sheetType => sheetType.DisplayOrder)
            .ThenBy(sheetType => sheetType.Name)
            .Select(SheetTypeMapper.Projection)
            .ToListAsync(cancellationToken);
    }

    public async Task<SheetTypeDto?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .AsNoTracking()
            .Where(sheetType => sheetType.Id == id)
            .Select(SheetTypeMapper.Projection)
            .SingleOrDefaultAsync(cancellationToken);
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

    public async Task<List<DbSheetTypeIdentifier>> ListIdentifiersForEditAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.SheetTypeIdentifiers
            .Where(identifier => identifier.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<SheetIdentifierDefinition>> ListIdentifierDefinitionsAsync(int sheetTypeId, CancellationToken cancellationToken)
    {
        var identifiers = await db.SheetTypeIdentifiers
            .AsNoTracking()
            .Where(identifier => identifier.SheetTypeId == sheetTypeId)
            .OrderBy(identifier => identifier.DisplayOrder)
            .ThenBy(identifier => identifier.Id)
            .ToListAsync(cancellationToken);

        return identifiers
            .Select(identifier => new SheetIdentifierDefinition(identifier.Id, identifier.Name, identifier.Kind, identifier.IsRequired, identifier.Options ?? []))
            .ToList();
    }

    public async Task<bool> IsIdentifierInUseAsync(int identifierId, CancellationToken cancellationToken)
    {
        return await db.SheetIdentifierValues.AnyAsync(value => value.SheetTypeIdentifierId == identifierId, cancellationToken);
    }

    public void Add(DbSheetType sheetType)
    {
        db.SheetTypes.Add(sheetType);
    }

    public void AddIdentifier(DbSheetTypeIdentifier identifier)
    {
        db.SheetTypeIdentifiers.Add(identifier);
    }

    public void RemoveIdentifier(DbSheetTypeIdentifier identifier)
    {
        db.SheetTypeIdentifiers.Remove(identifier);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
    }
}
