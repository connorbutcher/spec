using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.CellTypes;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Data.Repositories.CellTypes;

public sealed class CellTypeRepository(PuSpecSheetDbContext db) : ICellTypeRepository
{
    public async Task<List<CellType>> ListWithOptionsAsync(CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .OrderBy(cellType => cellType.DisplayOrder)
            .ThenBy(cellType => cellType.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<CellType?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .Include(candidate => candidate.Options)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await db.CellTypes.AnyAsync(cellType => cellType.Id == id, cancellationToken);
    }

    public async Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        return await db.CellTypes.AnyAsync(
            cellType => cellType.Name == name && cellType.Id != exceptId,
            cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(CancellationToken cancellationToken)
    {
        return await db.CellTypes.MaxAsync(cellType => (int?)cellType.DisplayOrder, cancellationToken);
    }

    public async Task<int?> FindDefaultIdAsync(CancellationToken cancellationToken)
    {
        return await db.CellTypes
            .OrderBy(cellType => cellType.Kind == CellKind.Text ? 0 : 1)
            .ThenBy(cellType => cellType.DisplayOrder)
            .Select(cellType => (int?)cellType.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> CountUsageByTypeAsync(CancellationToken cancellationToken)
    {
        return await db.TemplateCells
            .GroupBy(cell => cell.CellTypeId)
            .Select(group => new { CellTypeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.CellTypeId, entry => entry.Count, cancellationToken);
    }

    public async Task<int> CountUsageAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        return await db.TemplateCells.CountAsync(cell => cell.CellTypeId == cellTypeId, cancellationToken);
    }

    public async Task<bool> HasSheetCellsAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        var cellIds = db.TemplateCells
            .Where(cell => cell.CellTypeId == cellTypeId)
            .Select(cell => cell.Id);

        // Values hang off sheet cells, and a sheet cell exists for every template cell a sheet uses.
        return await db.SheetCells.AnyAsync(cell => cellIds.Contains(cell.TemplateCellId), cancellationToken);
    }

    public async Task<bool> AnyOptionChosenAsync(IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken)
    {
        return await db.OptionValues.AnyAsync(value => optionIds.Contains(value.CellTypeOptionId), cancellationToken);
    }

    public async Task ClearConfigurationOverridesAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        await db.TemplateCells
            .Where(cell => cell.CellTypeId == cellTypeId && cell.ConfigurationOverride != null)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(cell => cell.ConfigurationOverride, (CellConfiguration?)null),
                cancellationToken);
    }

    public void Add(CellType cellType)
    {
        db.CellTypes.Add(cellType);
    }

    public void Remove(CellType cellType)
    {
        db.CellTypes.Remove(cellType);
    }
}
