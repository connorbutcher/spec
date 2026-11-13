using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>Sheets, one per phase and sheet type, and the versions published of them.</summary>
public static class SheetQueries
{
    /// <summary>The sheet, read only; null when there is no such sheet.</summary>
    public static async Task<DbSheet?> FindAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The sheet, tracked for editing; null when there is no such sheet.</summary>
    public static async Task<DbSheet?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Sheets.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public static async Task<bool> ExistsAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Sheets.AnyAsync(sheet => sheet.Id == id, cancellationToken);
    }

    /// <summary>The id of the phase's sheet of the sheet type; null when it hasn't been created yet.</summary>
    public static async Task<int?> FindIdAsync(PuSpecSheetDbContext db, int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .Where(sheet => sheet.PhaseId == phaseId && sheet.SheetTypeId == sheetTypeId)
            .Select(sheet => (int?)sheet.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The sheet's versions with who published each, read only, oldest first.</summary>
    public static async Task<List<DbSheetVersion>> ListVersionsAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetVersions
            .AsNoTracking()
            .Include(version => version.PublishedBy)
            .Where(version => version.SheetId == sheetId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The highest version number of the sheet, or 0 when nothing has been published.</summary>
    public static async Task<int> GetLastVersionNumberAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetVersions
            .Where(version => version.SheetId == sheetId)
            .MaxAsync(version => (int?)version.VersionNumber, cancellationToken) ?? 0;
    }

    /// <summary>
    /// Deletes the sheet's tables, sections, rows and column blocks that have no revisions left (they only
    /// ever existed as drafts that are gone), straight in the database.
    /// </summary>
    public static async Task DeleteItemsWithoutRevisionsAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        var orphanBlockIds = await db.SheetColumnBlocks
            .Where(block => block.SheetTable.SheetId == sheetId && !block.Revisions.Any())
            .Select(block => block.Id)
            .ToListAsync(cancellationToken);
        await SheetColumnBlockDeletion.DeleteAsync(db, orphanBlockIds, cancellationToken);
        await db.SheetRows
            .Where(row => row.SheetSection.SheetTable.SheetId == sheetId && !row.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
        await db.SheetSections
            .Where(section => section.SheetTable.SheetId == sheetId && !section.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
        await db.SheetTables
            .Where(table => table.SheetId == sheetId && !table.Revisions.Any())
            .ExecuteDeleteAsync(cancellationToken);
    }

    /// <summary>Whether any sheet has a version published with the note.</summary>
    public static async Task<bool> AnyVersionWithNoteAsync(PuSpecSheetDbContext db, string note, CancellationToken cancellationToken)
    {
        return await db.SheetVersions.AnyAsync(version => version.Note == note, cancellationToken);
    }
}
