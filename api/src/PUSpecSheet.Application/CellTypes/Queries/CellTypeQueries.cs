using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.CellTypes;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Application.CellTypes.Queries;

/// <summary>The cell types template cells are made from, with their options, and where they are used.</summary>
public static class CellTypeQueries
{
    /// <summary>Every cell type with its options, read only, in display order then name.</summary>
    public static async Task<List<DbCellType>> ListWithOptionsAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .OrderBy(cellType => cellType.DisplayOrder)
            .ThenBy(cellType => cellType.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The cell type with its options, tracked for editing; null when there is no such type.</summary>
    public static async Task<DbCellType?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .Include(candidate => candidate.Options)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public static async Task<bool> ExistsAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.CellTypes.AnyAsync(cellType => cellType.Id == id, cancellationToken);
    }

    /// <summary>Whether another cell type than <paramref name="exceptId"/> already has the name.</summary>
    public static async Task<bool> NameTakenAsync(PuSpecSheetDbContext db, string name, int? exceptId, CancellationToken cancellationToken)
    {
        return await db.CellTypes.AnyAsync(
            cellType => cellType.Name == name && cellType.Id != exceptId,
            cancellationToken);
    }

    /// <summary>The highest display order in use, or null when there are no cell types.</summary>
    public static async Task<int?> GetLastDisplayOrderAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.CellTypes.MaxAsync(cellType => (int?)cellType.DisplayOrder, cancellationToken);
    }

    /// <summary>The type new cells get when none is chosen: the first Text type, else the first type; null when there are none.</summary>
    public static async Task<int?> FindDefaultIdAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .OrderBy(cellType => cellType.Kind == CellKind.Text ? 0 : 1)
            .ThenBy(cellType => cellType.DisplayOrder)
            .Select(cellType => (int?)cellType.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>How many template cells use each cell type that is used at all, by cell type id.</summary>
    public static async Task<Dictionary<int, int>> CountUsageByTypeAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.TemplateCells
            .GroupBy(cell => cell.CellTypeId)
            .Select(group => new { CellTypeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.CellTypeId, entry => entry.Count, cancellationToken);
    }

    /// <summary>How many template cells use the cell type.</summary>
    public static async Task<int> CountUsageAsync(PuSpecSheetDbContext db, int cellTypeId, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.CountAsync(cell => cell.CellTypeId == cellTypeId, cancellationToken);
    }

    /// <summary>Whether any sheet holds a cell made from the cell type.</summary>
    public static async Task<bool> HasSheetCellsAsync(PuSpecSheetDbContext db, int cellTypeId, CancellationToken cancellationToken)
    {
        var cellIds = db.TemplateCells
            .Where(cell => cell.CellTypeId == cellTypeId)
            .Select(cell => cell.Id);

        // Values hang off sheet cells, and a sheet cell exists for every template cell a sheet uses.
        return await db.SheetCells.AnyAsync(cell => cellIds.Contains(cell.TemplateCellId), cancellationToken);
    }

    /// <summary>Whether any of the options has been chosen on a sheet.</summary>
    public static async Task<bool> AnyOptionChosenAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken)
    {
        return await db.OptionValues.AnyAsync(value => optionIds.Contains(value.CellTypeOptionId), cancellationToken);
    }

    /// <summary>Clears the configuration overrides of every template cell of the type, straight in the database.</summary>
    public static async Task ClearConfigurationOverridesAsync(PuSpecSheetDbContext db, int cellTypeId, CancellationToken cancellationToken)
    {
        await db.TemplateCells
            .Where(cell => cell.CellTypeId == cellTypeId && cell.ConfigurationOverride != null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(cell => cell.ConfigurationOverride, (CellConfiguration?)null),
                cancellationToken);
    }

    /// <summary>Every cell type with its options, read only, by id.</summary>
    public static async Task<Dictionary<int, DbCellType>> GetAllWithOptionsByIdAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .ToDictionaryAsync(cellType => cellType.Id, cancellationToken);
    }

    /// <summary>Which of the names cell types already have.</summary>
    public static async Task<List<string>> ListNamesInUseAsync(PuSpecSheetDbContext db, IReadOnlyCollection<string> names, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .Where(cellType => names.Contains(cellType.Name))
            .Select(cellType => cellType.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The cell types with any of the names, with their options, read only.</summary>
    public static async Task<List<DbCellType>> ListByNamesAsync(PuSpecSheetDbContext db, IReadOnlyCollection<string> names, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .Where(cellType => names.Contains(cellType.Name))
            .ToListAsync(cancellationToken);
    }
}
