using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.Abstractions.CellTypes.Configurations;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.DataAccess.CellTypes;

public sealed class CellTypeDataService : ICellTypeDataService
{
    private readonly PuSpecSheetDbContext _db;

    public CellTypeDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbCellType>> ListAsync(CancellationToken cancellationToken)
    {
        return await _db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .OrderBy(cellType => cellType.DisplayOrder)
            .ThenBy(cellType => cellType.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, DbCellType>> ListByIdAsync(CancellationToken cancellationToken)
    {
        return await _db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .ToDictionaryAsync(cellType => cellType.Id, cancellationToken);
    }

    public async Task<List<DbCellType>> ListByNamesAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken)
    {
        return await _db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .Where(cellType => names.Contains(cellType.Name))
            .ToListAsync(cancellationToken);
    }

    public async Task<List<string>> ListNamesInUseAsync(IReadOnlyCollection<string> names, CancellationToken cancellationToken)
    {
        return await _db.CellTypes
            .Where(cellType => names.Contains(cellType.Name))
            .Select(cellType => cellType.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbCellType?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.CellTypes
            .Include(cellType => cellType.Options)
            .SingleOrDefaultAsync(cellType => cellType.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.CellTypes.AnyAsync(cellType => cellType.Id == id, cancellationToken);
    }

    public async Task<bool> NameTakenAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        return await _db.CellTypes.AnyAsync(cellType => cellType.Name == name && cellType.Id != exceptId, cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(CancellationToken cancellationToken)
    {
        return await _db.CellTypes.MaxAsync(cellType => (int?)cellType.DisplayOrder, cancellationToken);
    }

    public async Task<int> GetDefaultIdAsync(CancellationToken cancellationToken)
    {
        var id = await _db.CellTypes
            .OrderBy(cellType => cellType.Kind == CellKind.Text ? 0 : 1)
            .ThenBy(cellType => cellType.DisplayOrder)
            .Select(cellType => (int?)cellType.Id)
            .FirstOrDefaultAsync(cancellationToken);

        return id ?? throw new ConflictException("There are no cell types yet. Add a cell type first.");
    }

    public async Task<Dictionary<int, int>> CountUsageByTypeAsync(CancellationToken cancellationToken)
    {
        return await _db.TemplateCells
            .GroupBy(cell => cell.CellTypeId)
            .Select(group => new { CellTypeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.CellTypeId, entry => entry.Count, cancellationToken);
    }

    public async Task<int> CountUsageAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        return await _db.TemplateCells.CountAsync(cell => cell.CellTypeId == cellTypeId, cancellationToken);
    }

    public async Task<bool> HasSheetCellsAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        // Values hang off sheet cells, and a sheet cell exists for every template cell a sheet uses.
        return await _db.SheetCells.AnyAsync(cell => cell.TemplateCell.CellTypeId == cellTypeId, cancellationToken);
    }

    public async Task<bool> AnyOptionChosenAsync(IReadOnlyCollection<int> optionIds, CancellationToken cancellationToken)
    {
        return await _db.OptionValues.AnyAsync(value => optionIds.Contains(value.CellTypeOptionId), cancellationToken);
    }

    public void Add(DbCellType cellType)
    {
        _db.CellTypes.Add(cellType);
    }

    public void Remove(DbCellType cellType)
    {
        _db.CellTypes.Remove(cellType);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task SaveClearingConfigurationOverridesAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        await _db.TemplateCells
            .Where(cell => cell.CellTypeId == cellTypeId && cell.ConfigurationOverride != null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(cell => cell.ConfigurationOverride, (CellConfiguration?)null), cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }
}
