using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.CellTypes;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.CellTypes;

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

        CellTypeSettings.Apply(request, cellType);
        await db.SaveChangesAsync(cancellationToken);

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

    private Task<int> CountUsageAsync(int id, CancellationToken cancellationToken)
    {
        return db.TemplateCells.CountAsync(cell => cell.CellTypeId == id, cancellationToken);
    }
}
