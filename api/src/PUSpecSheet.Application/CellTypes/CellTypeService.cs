using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.CellTypes;
using PUSpecSheet.Contracts.CellTypes;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.CellTypes;

public sealed class CellTypeService(ICellTypeRepository cellTypes, IUnitOfWork unitOfWork) : ICellTypeService
{
    public async Task<IReadOnlyList<CellTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await cellTypes.ListWithOptionsAsync(cancellationToken);
        var usage = await cellTypes.CountUsageByTypeAsync(cancellationToken);

        return all
            .Select(cellType => cellType.ToDto(usage.GetValueOrDefault(cellType.Id)))
            .ToList();
    }

    public async Task<CellTypeDto> CreateAsync(SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        await EnsureNameIsFreeAsync(request.Name, null, cancellationToken);

        var lastOrder = await cellTypes.GetLastDisplayOrderAsync(cancellationToken);
        var cellType = new CellType { DisplayOrder = (lastOrder ?? 0) + 1 };
        CellTypeSettings.Apply(request, cellType);

        cellTypes.Add(cellType);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        return cellType.ToDto(0);
    }

    public async Task<CellTypeDto> UpdateAsync(int id, SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        var cellType = await FindAsync(id, cancellationToken);
        await EnsureNameIsFreeAsync(request.Name, id, cancellationToken);
        await EnsureSheetValuesKeepMeaningAsync(cellType, request, cancellationToken);

        var kindChanged = request.Kind != cellType.Kind;
        CellTypeSettings.Apply(request, cellType);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        if (kindChanged)
        {
            // Cells' configuration overrides were for the old kind, so they no longer mean anything.
            await cellTypes.ClearConfigurationOverridesAsync(id, cancellationToken);
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

        cellTypes.Remove(cellType);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<CellType> FindAsync(int id, CancellationToken cancellationToken)
    {
        var cellType = await cellTypes.FindForEditAsync(id, cancellationToken);

        if (cellType is null)
        {
            throw new NotFoundException($"Cell type {id} was not found.");
        }

        return cellType;
    }

    private async Task EnsureNameIsFreeAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        var taken = await cellTypes.NameTakenAsync(trimmed, exceptId, cancellationToken);

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
        if (request.Kind != cellType.Kind && await cellTypes.HasSheetCellsAsync(cellType.Id, cancellationToken))
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
            && await cellTypes.AnyOptionChosenAsync(removedIds, cancellationToken))
        {
            throw new ConflictException("An option you removed has been chosen on a sheet, so it has to stay.");
        }
    }

    private Task<int> CountUsageAsync(int id, CancellationToken cancellationToken)
    {
        return cellTypes.CountUsageAsync(id, cancellationToken);
    }
}
