using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The rows of sheet sections.</summary>
public static class SheetRowQueries
{
    /// <summary>
    /// The sheet's rows with their cells, read only. The cells' template cells are not loaded: the caller
    /// attaches them from template rows it reads anyway, rather than reading each again for every cell.
    /// </summary>
    public static async Task<List<DbSheetRow>> ListOnSheetAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .AsNoTracking()
            .Include(row => row.Cells)
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// The row with its section and table, and its cells with their template cells and cell types (and the
    /// types' options when asked), read only; null when there is no such row.
    /// </summary>
    public static async Task<DbSheetRow?> FindWithCellTypesAsync(PuSpecSheetDbContext db, int id, bool withOptions, CancellationToken cancellationToken)
    {
        var query = db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable);
        var withTypes = query.Include(candidate => candidate.Cells)
            .ThenInclude(cell => cell.TemplateCell)
            .ThenInclude(templateCell => templateCell.CellType);
        IQueryable<DbSheetRow> withCells = withOptions
            ? withTypes.ThenInclude(cellType => cellType.Options)
            : withTypes;
        return await withCells
            .AsSplitQuery()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The row with its section, the section's template section and table, read only; null when there is no such row.</summary>
    public static async Task<DbSheetRow?> FindWithSectionAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .AsNoTracking()
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.TemplateSection)
            .Include(candidate => candidate.SheetSection)
            .ThenInclude(section => section.SheetTable)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The sheet the row is on; null when there is no such row.</summary>
    public static async Task<int?> FindSheetIdAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => (int?)row.SheetSection.SheetTable.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The section the row is in; null when there is no such row.</summary>
    public static async Task<int?> FindSectionIdAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => (int?)row.SheetSectionId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The row's section and table; null when there is no such row.</summary>
    public static async Task<SheetRowPlace?> FindPlaceAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == id)
            .Select(row => new SheetRowPlace(row.SheetSectionId, row.SheetSection.SheetTableId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The ids of the section's rows.</summary>
    public static async Task<List<int>> ListIdsInSectionAsync(PuSpecSheetDbContext db, int sectionId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(candidate => candidate.SheetSectionId == sectionId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The ids of the rows in the sections.</summary>
    public static async Task<List<int>> ListIdsInSectionsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => sectionIds.Contains(row.SheetSectionId))
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The ids of the table's rows.</summary>
    public static async Task<List<int>> ListIdsOnTableAsync(PuSpecSheetDbContext db, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId)
            .Select(row => row.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The template rows the rows are made from.</summary>
    public static async Task<List<int>> ListTemplateRowIdsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => ids.Contains(row.Id))
            .Select(row => row.TemplateRowId)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    /// <summary>The table's rows made from any of the template rows, with the template row of each, by row id.</summary>
    public static async Task<Dictionary<int, int>> GetTemplateRowsOnTableAsync(PuSpecSheetDbContext db, int tableId, IReadOnlyCollection<int> templateRowIds, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.SheetSection.SheetTableId == tableId && templateRowIds.Contains(row.TemplateRowId))
            .ToDictionaryAsync(row => row.Id, row => row.TemplateRowId, cancellationToken);
    }

    /// <summary>Deletes the row straight in the database; its cells, revisions and values cascade.</summary>
    public static async Task DeleteAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        await db.SheetRows.Where(candidate => candidate.Id == id).ExecuteDeleteAsync(cancellationToken);
    }
}
