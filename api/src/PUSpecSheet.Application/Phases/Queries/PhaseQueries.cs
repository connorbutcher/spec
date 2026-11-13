using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.Application.Phases.Queries;

/// <summary>Phases, such as V6 and its builds, and the sheet types each one has.</summary>
public static class PhaseQueries
{
    /// <summary>Every phase with its sheet types, read only, in display order then code.</summary>
    public static async Task<List<DbPhase>> ListWithSheetTypesAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.Phases
            .AsNoTracking()
            .Include(phase => phase.SheetTypes)
            .OrderBy(phase => phase.DisplayOrder)
            .ThenBy(phase => phase.Code)
            .ToListAsync(cancellationToken);
    }

    /// <summary>Every phase with its sheet types, tracked for editing.</summary>
    public static async Task<List<DbPhase>> ListForEditAsync(PuSpecSheetDbContext db, CancellationToken cancellationToken)
    {
        return await db.Phases
            .Include(candidate => candidate.SheetTypes)
            .ToListAsync(cancellationToken);
    }

    /// <summary>The phase with its sheet types, read only; null when there is no such phase.</summary>
    public static async Task<DbPhase?> FindWithSheetTypesAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Phases
            .AsNoTracking()
            .Include(candidate => candidate.SheetTypes)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    /// <summary>The phase with its sheet types, tracked for editing; null when there is no such phase.</summary>
    public static async Task<DbPhase?> FindForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Phases
            .Include(candidate => candidate.SheetTypes)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public static async Task<bool> ExistsAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.Phases.AnyAsync(phase => phase.Id == id, cancellationToken);
    }

    public static async Task<bool> CodeExistsAsync(PuSpecSheetDbContext db, string code, CancellationToken cancellationToken)
    {
        return await db.Phases.AnyAsync(phase => phase.Code == code, cancellationToken);
    }

    /// <summary>The highest display order among the children of a parent (or the top level), or null when there are none.</summary>
    public static async Task<int?> GetLastDisplayOrderAsync(PuSpecSheetDbContext db, int? parentPhaseId, CancellationToken cancellationToken)
    {
        return await db.Phases
            .Where(phase => phase.ParentPhaseId == parentPhaseId)
            .MaxAsync(phase => (int?)phase.DisplayOrder, cancellationToken);
    }

    /// <summary>
    /// The name of the first sheet type, in display order, among those asked for whose sheet in the phase
    /// already has tables; null when none has.
    /// </summary>
    public static async Task<string?> FindSheetTypeWithTablesAsync(PuSpecSheetDbContext db, int phaseId, IReadOnlyCollection<int> sheetTypeIds, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .Where(sheet => sheet.PhaseId == phaseId && sheetTypeIds.Contains(sheet.SheetTypeId) && sheet.Tables.Any())
            .OrderBy(sheet => sheet.SheetType.DisplayOrder)
            .Select(sheet => sheet.SheetType.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    /// <summary>Whether the phase offers the sheet type.</summary>
    public static async Task<bool> HasSheetTypeAsync(PuSpecSheetDbContext db, int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.PhaseSheetTypes
            .AnyAsync(link => link.PhaseId == phaseId && link.SheetTypeId == sheetTypeId, cancellationToken);
    }

    /// <summary>The phases with any of the codes, with their sheet types, tracked for editing.</summary>
    public static async Task<List<DbPhase>> ListByCodesForEditAsync(PuSpecSheetDbContext db, IReadOnlyCollection<string> codes, CancellationToken cancellationToken)
    {
        return await db.Phases
            .Include(phase => phase.SheetTypes)
            .Where(phase => codes.Contains(phase.Code))
            .ToListAsync(cancellationToken);
    }
}
