using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.CellTypes;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;

namespace PUSpecSheet.Application.CellTypes;

public sealed class CellTypeService(PuSpecSheetDbContext db) : ICellTypeService
{
    public async Task<IReadOnlyList<CellTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var cellTypes = await db.CellTypes
            .AsNoTracking()
            .Include(cellType => cellType.Options)
            .OrderBy(cellType => cellType.DisplayOrder)
            .ThenBy(cellType => cellType.Name)
            .ToListAsync(cancellationToken);

        var usage = await db.TemplateCells
            .GroupBy(cell => cell.CellTypeId)
            .Select(group => new { CellTypeId = group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.CellTypeId, entry => entry.Count, cancellationToken);

        return cellTypes
            .Select(cellType => cellType.ToDto(usage.GetValueOrDefault(cellType.Id)))
            .ToList();
    }

    public async Task<CellTypeDto> CreateAsync(SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        await EnsureNameIsFreeAsync(request.Name, null, cancellationToken);

        var lastOrder = await db.CellTypes.MaxAsync(cellType => (int?)cellType.DisplayOrder, cancellationToken);
        var cellType = new CellType { DisplayOrder = (lastOrder ?? 0) + 1 };
        CellTypeSettings.Apply(request, cellType);

        db.CellTypes.Add(cellType);
        await db.SaveChangesAsync(cancellationToken);

        return cellType.ToDto(0);
    }

    public async Task<CellTypeDto> UpdateAsync(int id, SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        var cellType = await FindAsync(id, cancellationToken);
        await EnsureNameIsFreeAsync(request.Name, id, cancellationToken);
        await EnsureSheetValuesKeepMeaningAsync(cellType, request, cancellationToken);

        var kindChanged = request.Kind != cellType.Kind;
        CellTypeSettings.Apply(request, cellType);

        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        if (kindChanged)
        {
            // Cells' configuration overrides were for the old kind, so they no longer mean anything.
            await db.TemplateCells
                .Where(cell => cell.CellTypeId == id && cell.ConfigurationOverride != null)
                .ExecuteUpdateAsync(
                    setters => setters.SetProperty(cell => cell.ConfigurationOverride, (CellConfiguration?)null),
                    cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
        return cellType.ToDto(await CountUsageAsync(id, cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var cellType = await FindAsync(id, cancellationToken);

        var usage = await CountUsageAsync(id, cancellationToken);
        if (usage > 0)
        {
            var cells = usage == 1 ? "1 cell" : $"{usage} cells";
            throw new ConflictException($"\"{cellType.Name}\" is used by {cells}. Change those cells to another type first.");
        }

        db.CellTypes.Remove(cellType);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task<CellType> FindAsync(int id, CancellationToken cancellationToken)
    {
        var cellType = await db.CellTypes
            .Include(candidate => candidate.Options)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);

        if (cellType is null)
        {
            throw new NotFoundException($"Cell type {id} was not found.");
        }

        return cellType;
    }

    private async Task EnsureNameIsFreeAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        var taken = await db.CellTypes.AnyAsync(
            cellType => cellType.Name == trimmed && cellType.Id != exceptId,
            cancellationToken);

        if (taken)
        {
            throw new ConflictException($"A cell type called \"{trimmed}\" already exists.");
        }
    }

    /// <summary>
    /// Sheet values are stored by kind, and dropdown values point at an option, so a type whose cells
    /// already hold sheet values can't change kind, and an option chosen on a sheet can't be removed.
    /// </summary>
    private async Task EnsureSheetValuesKeepMeaningAsync(
        CellType cellType,
        SaveCellTypeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Kind != cellType.Kind && await HasSheetValuesAsync(cellType.Id, cancellationToken))
        {
            throw new ConflictException(
                $"Sheets already have values for \"{cellType.Name}\" cells, so its kind can't change.");
        }

        var keptValues = request.Kind.IsDropdown()
            ? (request.Options ?? []).Select(option => option.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];

        var removedIds = cellType.Options
            .Where(option => !keptValues.Contains(option.Value))
            .Select(option => option.Id)
            .ToList();

        if (removedIds.Count > 0
            && await db.OptionValues.AnyAsync(value => removedIds.Contains(value.CellTypeOptionId), cancellationToken))
        {
            throw new ConflictException("An option you removed has been chosen on a sheet, so it has to stay.");
        }
    }

    private async Task<bool> HasSheetValuesAsync(int cellTypeId, CancellationToken cancellationToken)
    {
        var cellIds = db.TemplateCells
            .Where(cell => cell.CellTypeId == cellTypeId)
            .Select(cell => cell.Id);

        // Values now hang off sheet cells, and a sheet cell exists for every template cell a sheet uses.
        return await db.SheetCells.AnyAsync(cell => cellIds.Contains(cell.TemplateCellId), cancellationToken);
    }

    private Task<int> CountUsageAsync(int id, CancellationToken cancellationToken)
    {
        return db.TemplateCells.CountAsync(cell => cell.CellTypeId == id, cancellationToken);
    }
}
