using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets;

public sealed class SheetTableDataService : ISheetTableDataService
{
    private readonly PuSpecSheetDbContext _db;

    public SheetTableDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public async Task<List<DbSheetTable>> ListOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetTables
            .AsNoTracking()
            .Include(table => table.TableTemplateVersion)
            .ThenInclude(version => version.TableTemplate)
            .Where(table => table.SheetId == sheetId)
            .ToListAsync(cancellationToken);
    }

    public async Task<DbSheetTable?> FindForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetTables.SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<DbSheetTable?> FindWithTemplateVersionAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetTables
            .AsNoTracking()
            .Include(candidate => candidate.TableTemplateVersion)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public async Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetTables
            .Where(table => table.Id == id)
            .Select(table => (int?)table.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<List<int>> ListIdsOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _db.SheetTables
            .Where(candidate => candidate.SheetId == sheetId)
            .Select(candidate => candidate.Id)
            .ToListAsync(cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        await _db.SheetTables.Where(table => table.Id == id).ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<List<DbSheetTableRevision>> ListVisibleOnSheetAsync(int sheetId, int userId, CancellationToken cancellationToken)
    {
        return await _db.SheetTableRevisions
            .Where(revision => revision.SheetTable.SheetId == sheetId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PendingRemoval>> ListPendingRemovalsAsync(int tableId, CancellationToken cancellationToken)
    {
        return await _db.SheetTableRevisions
            .Where(revision => revision.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new PendingRemoval
            {
                Container = RemovedContainer.Table,
                AuthorUserId = revision.AuthorUserId,
                AwaitingApproval = revision.SheetSubmissionId != null,
            })
            .ToListAsync(cancellationToken);
    }
}
