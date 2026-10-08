using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowCheckouts(PuSpecSheetDbContext db, TimeProvider clock) : IRowCheckouts
{
    public async Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        return await DraftsOf([rowId])
            .AsNoTracking()
            .Select(draft => new RowCheckout(draft.SheetRow.SheetSection.SheetTable.SheetId, draft.AuthorUserId))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> RowExistsAsync(int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRows.AnyAsync(row => row.Id == rowId, cancellationToken);
    }

    public async Task<IReadOnlyDictionary<int, int>> HoldersAsync(IReadOnlyCollection<int> rowIds, CancellationToken cancellationToken)
    {
        return await DraftsOf(rowIds)
            .AsNoTracking()
            .ToDictionaryAsync(draft => draft.SheetRowId, draft => draft.AuthorUserId, cancellationToken);
    }

    public async Task<bool> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        var draft = await DraftsOf([rowId]).SingleOrDefaultAsync(cancellationToken);
        if (draft is null || draft.AuthorUserId != fromUserId)
        {
            return false;
        }

        // The draft's row version makes this fail, as a conflict, if its holder saves at the same moment.
        draft.AuthorUserId = toUserId;
        draft.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await db.SaveSheetChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    /// <summary>A row has at most one draft, so this is at most one revision per row.</summary>
    private IQueryable<SheetRowRevision> DraftsOf(IReadOnlyCollection<int> rowIds)
    {
        return db.SheetRowRevisions
            .Where(revision => rowIds.Contains(revision.SheetRowId) && revision.Status == RevisionStatus.Draft);
    }
}
