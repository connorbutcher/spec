using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>Sheets, one per phase, sheet type and identity, and the versions published of them.</summary>
public static class SheetQueries
{
    /// <summary>The sheet, read only; null when there is no such sheet.</summary>
    public static async Task<DbSheet?> FindAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The sheet with its identifier values, read only; null when there is no such sheet.</summary>
    public static async Task<DbSheet?> FindWithIdentifiersAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .Include(sheet => sheet.IdentifierValues)
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

    /// <summary>
    /// The id of the phase's sheet of the sheet type with no identifier values (the only one, for a sheet type
    /// without identifiers); null when it hasn't been created.
    /// </summary>
    public static async Task<int?> FindIdAsync(PuSpecSheetDbContext db, int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .Where(sheet => sheet.PhaseId == phaseId && sheet.SheetTypeId == sheetTypeId && !sheet.IdentifierValues.Any())
            .OrderBy(sheet => sheet.Id)
            .Select(sheet => (int?)sheet.Id)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>The phase's sheets of the sheet type with their identifier values, read only.</summary>
    public static async Task<List<DbSheet>> ListWithIdentifiersAsync(PuSpecSheetDbContext db, int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .Include(sheet => sheet.IdentifierValues)
            .Where(sheet => sheet.PhaseId == phaseId && sheet.SheetTypeId == sheetTypeId)
            .ToListAsync(cancellationToken);
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

    /// <summary>Whether any sheet has a version published with the note.</summary>
    public static async Task<bool> AnyVersionWithNoteAsync(PuSpecSheetDbContext db, string note, CancellationToken cancellationToken)
    {
        return await db.SheetVersions.AnyAsync(version => version.Note == note, cancellationToken);
    }
}
