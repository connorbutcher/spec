using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowCheckouts(PuSpecSheetDbContext db, RowValueStore valueStore, TimeProvider clock) : IRowCheckouts
{
    public async Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        var revisions = await db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == rowId)
            .CurrentAndDrafts()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        var draft = revisions.SingleOrDefault(revision => revision.Status == RevisionStatus.Draft);
        if (draft is null)
        {
            return null;
        }

        var published = revisions.SingleOrDefault(revision => revision.Status == RevisionStatus.Published);
        var sheetId = await db.SheetRows
            .Where(row => row.Id == rowId)
            .Select(row => row.SheetSection.SheetTable.SheetId)
            .SingleAsync(cancellationToken);

        return new RowCheckout(sheetId, draft.AuthorUserId, await HasChangesAsync(draft, published, cancellationToken));
    }

    public async Task<bool> RowExistsAsync(int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRows.AnyAsync(row => row.Id == rowId, cancellationToken);
    }

    public async Task<bool> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        var draft = await db.SheetRowRevisions
            .SingleOrDefaultAsync(
                revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft,
                cancellationToken);
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

    /// <summary>
    /// Whether a draft differs from what is published, by the same comparison that decides whether a draft
    /// is worth keeping (<see cref="RowDrafts"/>): its place, its existence and every cell's value. A row
    /// that has never been published is all change.
    /// </summary>
    private async Task<bool> HasChangesAsync(SheetRowRevision draft, SheetRowRevision? published, CancellationToken cancellationToken)
    {
        if (published is null || !SheetRevisionComparer.SameStructure(draft, published))
        {
            return true;
        }

        var values = await valueStore.LoadAsync([draft.Id, published.Id], cancellationToken);
        return !CellValuesComparer.Same(values.GetValueOrDefault(draft.Id), values.GetValueOrDefault(published.Id));
    }
}
