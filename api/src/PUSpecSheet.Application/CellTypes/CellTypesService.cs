using PUSpecSheet.Application.CellTypes.Queries;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.CellTypes;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.CellTypes;
using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.CellTypes;

public sealed class CellTypesService : ICellTypesService
{
    private readonly PuSpecSheetDbContext db;

    public CellTypesService(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<IReadOnlyList<CellTypeDto>> GetAllAsync(CancellationToken cancellationToken)
    {
        var all = await CellTypeQueries.ListWithOptionsAsync(db, cancellationToken);
        var usage = await CellTypeQueries.CountUsageByTypeAsync(db, cancellationToken);

        return all
            .Select(cellType => cellType.ToDto(usage.GetValueOrDefault(cellType.Id)))
            .ToList();
    }

    public async Task<CellTypeDto> CreateAsync(SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        await EnsureNameIsFreeAsync(request.Name, null, cancellationToken);

        var lastOrder = await CellTypeQueries.GetLastDisplayOrderAsync(db, cancellationToken);
        var cellType = new DbCellType { DisplayOrder = (lastOrder ?? 0) + 1 };
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
            await CellTypeQueries.ClearConfigurationOverridesAsync(db, id, cancellationToken);
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

    private async Task<DbCellType> FindAsync(int id, CancellationToken cancellationToken)
    {
        var cellType = await CellTypeQueries.FindForEditAsync(db, id, cancellationToken);

        if (cellType is null)
        {
            throw new NotFoundException($"Cell type {id} was not found.");
        }

        return cellType;
    }

    private async Task EnsureNameIsFreeAsync(string name, int? exceptId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        var taken = await CellTypeQueries.NameTakenAsync(db, trimmed, exceptId, cancellationToken);

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
        DbCellType cellType,
        SaveCellTypeRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Kind != cellType.Kind && await CellTypeQueries.HasSheetCellsAsync(db, cellType.Id, cancellationToken))
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
            && await CellTypeQueries.AnyOptionChosenAsync(db, removedIds, cancellationToken))
        {
            throw new ConflictException("An option you removed has been chosen on a sheet, so it has to stay.");
        }
    }

    private Task<int> CountUsageAsync(int id, CancellationToken cancellationToken)
    {
        return CellTypeQueries.CountUsageAsync(db, id, cancellationToken);
    }
}
