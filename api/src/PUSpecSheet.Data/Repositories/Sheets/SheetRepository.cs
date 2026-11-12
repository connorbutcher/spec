using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Data.Repositories.Sheets;

public sealed class SheetRepository(PuSpecSheetDbContext db) : ISheetRepository
{
    public async Task<Sheet?> FindAsync(int id, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<Sheet?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.Sheets.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<bool> ExistsAsync(int id, CancellationToken cancellationToken)
    {
        return await db.Sheets.AnyAsync(sheet => sheet.Id == id, cancellationToken);
    }

    public async Task<int?> FindIdAsync(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        return await db.Sheets
            .Where(sheet => sheet.PhaseId == phaseId && sheet.SheetTypeId == sheetTypeId)
            .Select(sheet => (int?)sheet.Id)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<List<SheetVersion>> ListVersionsAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetVersions
            .AsNoTracking()
            .Include(version => version.PublishedBy)
            .Where(version => version.SheetId == sheetId)
            .OrderBy(version => version.VersionNumber)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> GetLastVersionNumberAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await db.SheetVersions
            .Where(version => version.SheetId == sheetId)
            .MaxAsync(version => (int?)version.VersionNumber, cancellationToken) ?? 0;
    }

    public async Task DeleteItemsWithoutRevisionsAsync(int sheetId, CancellationToken cancellationToken)
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

    public async Task<bool> AnyVersionWithNoteAsync(string note, CancellationToken cancellationToken)
    {
        return await db.SheetVersions.AnyAsync(version => version.Note == note, cancellationToken);
    }

    public void Add(Sheet sheet)
    {
        db.Sheets.Add(sheet);
    }

    public void AddVersion(SheetVersion version)
    {
        db.SheetVersions.Add(version);
    }
}
