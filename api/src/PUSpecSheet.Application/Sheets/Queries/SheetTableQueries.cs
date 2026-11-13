using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The tables on sheets.</summary>
public static class SheetTableQueries
{
    /// <summary>The sheet's tables with the template version and template each is built from, read only.</summary>
    public static async Task<List<DbSheetTable>> ListOnSheetAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .AsNoTracking()
            .Include(table => table.TableTemplateVersion)
            .ThenInclude(version => version.TableTemplate)
            .Where(table => table.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The table, tracked for editing; null when there is no such table.</summary>
    public static async Task<DbSheetTable?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetTables.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The table with its template version, read only; null when there is no such table.</summary>
    public static async Task<DbSheetTable?> FindWithTemplateVersionAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .AsNoTracking()
            .Include(candidate => candidate.TableTemplateVersion)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The sheet the table is on; null when there is no such table.</summary>
    public static async Task<int?> FindSheetIdAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .Where(table => table.Id == id)
            .Select(table => (int?)table.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The ids of every table on the sheet.</summary>
    public static async Task<List<int>> ListIdsOnSheetAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetTables
            .Where(candidate => candidate.SheetId == sheetId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Deletes the table straight in the database; everything on it cascades.</summary>
    public static async Task DeleteAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        await db.SheetTables.Where(table => table.Id == id).ExecuteDeleteAsync(cancellationToken);
    }
}
