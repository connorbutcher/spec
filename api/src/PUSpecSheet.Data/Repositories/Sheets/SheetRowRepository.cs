using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Repositories.Sheets;

public sealed class SheetRowRepository(PuSpecSheetDbContext db) : ISheetRowRepository
{
    public async Task<List<SheetRow>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<SheetRow?> FindWithCellTypesAsync(int id, bool withOptions, CancellationToken cancellationToken)
    {
        var query = db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable);
        var withTypes = query.Include(candidate => candidate.Cells)
            .ThenInclude(cell => cell.TemplateCell)
            .ThenInclude(templateCell => templateCell.CellType);
        IQueryable<SheetRow> withCells = withOptions
            ? withTypes.ThenInclude(cellType => cellType.Options)
            : withTypes;
        return await withCells
            .AsSplitQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<SheetRow?> FindWithSectionAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.TemplateSection)
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => (int?)row.SheetSection.SheetTable.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<int?> FindSectionIdAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => (int?)row.SheetSectionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SheetRowPlace?> FindPlaceAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => new SheetRowPlace(row.SheetSectionId, row.SheetSection.SheetTableId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsInSectionAsync(int sectionId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(candidate => candidate.SheetSectionId == sectionId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsInSectionsAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => sectionIds.Contains(row.SheetSectionId))
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsOnTableAsync(int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<int>> ListTemplateRowIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => ids.Contains(row.Id))
            .Select(row => row.TemplateRowId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    public async Task<Dictionary<int, int>> GetTemplateRowsOnTableAsync(int tableId, IReadOnlyCollection<int> templateRowIds, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId && templateRowIds.Contains(row.TemplateRowId))
            .ToDictionaryAsync(row => row.Id, row => row.TemplateRowId, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await db.SheetRows.Where(candidate => candidate.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public void Add(SheetRow row)
    {
        db.SheetRows.Add(row);
    }
}
