using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Phases;

namespace PUSpecSheet.DataAccess.Phases;

public sealed class PhaseDataService : IPhaseDataService
{
    private readonly PuSpecSheetDbContext db;

    public PhaseDataService(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public async Task<List<DbPhase>> ListAsync(CancellationToken cancellationToken)
    {
        return await db.Phases
            .AsNoTracking()
            .Include(phase => phase.SheetTypes)
            .OrderBy(phase => phase.DisplayOrder)
            .ThenBy(phase => phase.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbPhase>> ListForEditAsync(CancellationToken cancellationToken)
    {
        return await db.Phases
            .Include(phase => phase.SheetTypes)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<DbPhase>> ListByCodesForEditAsync(IReadOnlyCollection<string> codes, CancellationToken cancellationToken)
    {
        return await db.Phases
            .Include(phase => phase.SheetTypes)
            .Where(phase => codes.Contains(phase.Code))
            .ToListAsync(cancellationToken);
    }

    public async Task<DbPhase?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.Phases
            .Include(phase => phase.SheetTypes)
            .SingleOrDefaultAsync(phase => phase.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await db.Phases.AnyAsync(phase => phase.Id == id, cancellationToken);
    }

    public async Task<bool> CodeExistsAsync(string code, CancellationToken cancellationToken)
    {
        return await db.Phases.AnyAsync(phase => phase.Code == code, cancellationToken);
    }

    public async Task<int?> GetLastDisplayOrderAsync(int? parentPhaseId, CancellationToken cancellationToken)
    {
        return await db.Phases
            .Where(phase => phase.ParentPhaseId == parentPhaseId)
            .MaxAsync(phase => (int?)phase.DisplayOrder, cancellationToken);
    }

    public async Task<bool> HasSheetTypeAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.PhaseSheetTypes.AnyAsync(link => link.PhaseId == phaseId && link.SheetTypeId == sheetTypeId, cancellationToken);
    }

    public async Task<string?> FindSheetTypeWithTablesAsync(int phaseId, IReadOnlyCollection<int> sheetTypeIds, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .Where(sheet => sheet.PhaseId == phaseId && sheetTypeIds.Contains(sheet.SheetTypeId) && sheet.Tables.Any())
            .OrderBy(sheet => sheet.SheetType.DisplayOrder)
            .Select(sheet => sheet.SheetType.Name)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public void Add(DbPhase phase)
    {
        db.Phases.Add(phase);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await db.SaveChangesAsync(cancellationToken);
    }
}
