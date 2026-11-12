using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The sections of sheet tables.</summary>
public static class SheetSectionQueries
{
    /// <summary>The sections of the sheet's tables with the template section each copies, read only.</summary>
    public static async Task<List<SheetSection>> ListOnSheetAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Where(section => section.SheetTable.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The section with its template section and table, read only; null when there is no such section.</summary>
    public static async Task<SheetSection?> FindWithTemplateAndTableAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Include(section => section.TemplateSection)
            .Include(section => section.SheetTable)
            .SingleOrDefaultAsync(section => section.Id == id, cancellationToken);
    }

    /// <summary>The section, read only, when it is on the table; null otherwise.</summary>
    public static async Task<SheetSection?> FindOnTableAsync(PuSpecSheetDbContext db, int id, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id && candidate.SheetTableId == tableId, cancellationToken);
    }

    /// <summary>Where each of the table's sections sits.</summary>
    public static async Task<List<SheetSectionPlace>> ListPlacesOnTableAsync(PuSpecSheetDbContext db, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new SheetSectionPlace(section.Id, section.TemplateSectionId, section.ParentSheetSectionId))
            .ToListAsync(cancellationToken);
    }

    /// <summary>The section each of the table's sections is inside (null at the top), by section id.</summary>
    public static async Task<Dictionary<int, int?>> GetParentsOnTableAsync(PuSpecSheetDbContext db, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Where(section => section.SheetTableId == tableId)
            .Select(section => new { section.Id, section.ParentSheetSectionId })
            .ToDictionaryAsync(section => section.Id, section => section.ParentSheetSectionId, cancellationToken);
    }

    /// <summary>The section each of the sections is inside (null at the top), by section id.</summary>
    public static async Task<Dictionary<int, int?>> GetParentsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .AsNoTracking()
            .Where(candidate => ids.Contains(candidate.Id))
            .Select(candidate => new { candidate.Id, candidate.ParentSheetSectionId })
            .ToDictionaryAsync(entry => entry.Id, entry => entry.ParentSheetSectionId, cancellationToken);
    }

    /// <summary>The ids of the table's sections under one parent (or at the top level).</summary>
    public static async Task<List<int>> ListSiblingIdsAsync(PuSpecSheetDbContext db, int tableId, int? parentSectionId, CancellationToken cancellationToken)
    {
        return await db.SheetSections
            .Where(candidate => candidate.SheetTableId == tableId && candidate.ParentSheetSectionId == parentSectionId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Deletes the sections straight in the database; their rows cascade.</summary>
    public static async Task DeleteAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> ids, CancellationToken cancellationToken)
    {
        await db.SheetSections.Where(candidate => ids.Contains(candidate.Id)).ExecuteDeleteAsync(cancellationToken);
    }
}
