using PUSpecSheet.Abstractions.CellTypes;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.Database.Mappers.CellTypes;
using PUSpecSheet.Database.Models.CellTypes;

namespace PUSpecSheet.DataAccess.CellTypes;

public sealed class CellTypesService : ICellTypesService
{
    private readonly ICellTypeDataService _cellTypes;

    public CellTypesService(ICellTypeDataService cellTypes)
    {
        _cellTypes = cellTypes;
    }

    public async Task<IReadOnlyList<CellTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await _cellTypes.ListAsync(cancellationToken);
        var usage = await _cellTypes.CountUsageByTypeAsync(cancellationToken);
        return all.Select(cellType => cellType.ToDto(usage.GetValueOrDefault(cellType.Id))).ToList();
    }

    public async Task<CellTypeDto> CreateAsync(SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        await EnsureNameIsFreeAsync(request.Name, null, cancellationToken);

        var lastOrder = await _cellTypes.GetLastDisplayOrderAsync(cancellationToken) ?? 0;
        var cellType = new DbCellType { DisplayOrder = lastOrder + 1 };
        CellTypeSettings.Apply(request, cellType);

        _cellTypes.Add(cellType);
        await _cellTypes.SaveAsync(cancellationToken);
        return cellType.ToDto(0);
    }

    public async Task<CellTypeDto> UpdateAsync(int id, SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        var cellType = await FindAsync(id, cancellationToken);
        await EnsureNameIsFreeAsync(request.Name, id, cancellationToken);
        await EnsureSheetValuesKeepMeaningAsync(cellType, request, cancellationToken);

        var kindChanged = request.Kind != cellType.Kind;
        CellTypeSettings.Apply(request, cellType);

        if (kindChanged)
        {
            // Cells' configuration overrides were for the old kind, so they no longer mean anything.
            await _cellTypes.SaveClearingConfigurationOverridesAsync(id, cancellationToken);
        }
        else
        {
            await _cellTypes.SaveAsync(cancellationToken);
        }

        return cellType.ToDto(await _cellTypes.CountUsageAsync(id, cancellationToken));
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var cellType = await FindAsync(id, cancellationToken);
        var usage = await _cellTypes.CountUsageAsync(id, cancellationToken);
        if (usage > 0)
        {
            var cells = usage == 1 ? "1 cell" : $"{usage} cells";
            throw new ConflictException($"\"{cellType.Name}\" is used by {cells}. Change those cells to another type first.");
        }

        _cellTypes.Remove(cellType);
        await _cellTypes.SaveAsync(cancellationToken);
    }

    private async Task<DbCellType> FindAsync(int id, CancellationToken cancellationToken)
    {
        return await _cellTypes.FindForEditAsync(id, cancellationToken)
            ?? throw new NotFoundException($"Cell type {id} was not found.");
    }

    private async Task EnsureNameIsFreeAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        if (await _cellTypes.NameTakenAsync(trimmed, exceptId, cancellationToken))
        {
            throw new ConflictException($"A cell type called \"{trimmed}\" already exists.");
        }
    }

    // Sheet values are stored by kind and dropdown values point at an option, so a type whose cells hold
    // sheet values can't change kind, and an option chosen on a sheet can't be removed.
    private async Task EnsureSheetValuesKeepMeaningAsync(DbCellType cellType, SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        if (request.Kind != cellType.Kind && await _cellTypes.HasSheetCellsAsync(cellType.Id, cancellationToken))
        {
            throw new ConflictException($"Sheets already have values for \"{cellType.Name}\" cells, so its kind can't change.");
        }

        var keptValues = request.Kind.IsDropdown()
            ? (request.Options ?? []).Select(option => option.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase)
            : [];
        var removedIds = cellType.Options
            .Where(option => !keptValues.Contains(option.Value))
            .Select(option => option.Id)
            .ToList();

        if (removedIds.Count > 0 && await _cellTypes.AnyOptionChosenAsync(removedIds, cancellationToken))
        {
            throw new ConflictException("An option you removed has been chosen on a sheet, so it has to stay.");
        }
    }
}
