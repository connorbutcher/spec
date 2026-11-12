using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.SheetTypes.Queries;

/// <summary>The kinds of sheet a phase can have, such as Specification or Parts.</summary>
public static class SheetTypeQueries
{
    /// <summary>Every sheet type, in display order then name.</summary>
    public static async Task<List<SheetTypeDto>> ListAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .AsNoTracking()
            .OrderBy(sheetType => sheetType.DisplayOrder)
            .ThenBy(sheetType => sheetType.Name)
            .Select(sheetType => new SheetTypeDto(sheetType.Id, sheetType.Name, sheetType.DisplayOrder))
            .ToListAsync(cancellationToken);
    }

    public static async Task<bool> ExistsAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetTypes.AnyAsync(sheetType => sheetType.Id == id, cancellationToken);
    }

    public static async Task<bool> NameExistsAsync(PuSpecSheetDbContext db, string name, CancellationToken cancellationToken)
    {
        return await db.SheetTypes.AnyAsync(sheetType => sheetType.Name == name, cancellationToken);
    }

    /// <summary>The highest display order in use, or null when there are no sheet types.</summary>
    public static async Task<int?> GetLastDisplayOrderAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.SheetTypes.MaxAsync(sheetType => (int?)sheetType.DisplayOrder, cancellationToken);
    }

    /// <summary>Which of the ids asked for are sheet types.</summary>
    public static async Task<List<int>> GetExistingIdsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .Where(sheetType => ids.Contains(sheetType.Id))
            .Select(sheetType => sheetType.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The id of the sheet type with the name. It must exist.</summary>
    public static async Task<int> GetIdByNameAsync(PuSpecSheetDbContext db, string name, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .Where(sheetType => sheetType.Name == name)
            .Select(sheetType => sheetType.Id)
            .SingleAsync(cancellationToken);
    }

    /// <summary>The ids of the sheet types with any of the names.</summary>
    public static async Task<List<int>> ListIdsByNamesAsync(PuSpecSheetDbContext db, IReadOnlyCollection<string> names, CancellationToken cancellationToken)
    {
        return await db.SheetTypes
            .Where(sheetType => names.Contains(sheetType.Name))
            .Select(sheetType => sheetType.Id)
            .ToListAsync(cancellationToken);
    }
}
