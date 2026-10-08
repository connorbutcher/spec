using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowCheckouts(
    PuSpecSheetDbContext db,
    RowValueStore valueStore,
    LiveRowCheckoutTracker live,
    SheetPresenceTracker presence,
    ISheetLiveNotifier notifier,
    TimeProvider clock) : IRowCheckouts
{
    public async Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        if (await FindDraftAsync(rowId, cancellationToken) is { } drafted)
        {
            return drafted;
        }

        // Someone who is only in the row has, by definition, changed nothing.
        return live.HolderOf(rowId) is { } holder
            ? new RowCheckout(holder.SheetId, holder.UserId, HasChanges: false)
            : null;
    }

    public async Task<RowCheckout?> FindDraftAsync(int rowId, CancellationToken cancellationToken)
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
        var sheetId = await SheetOfAsync(rowId, cancellationToken);
        return new RowCheckout(sheetId!.Value, draft.AuthorUserId, await HasChangesAsync(draft, published, cancellationToken));
    }

    public async Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRows
            .Where(row => row.Id == rowId)
            .Select(row => (int?)row.SheetSection.SheetTable.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        var movedDraft = await TransferDraftAsync(rowId, fromUserId, toUserId, cancellationToken);
        var movedLive = await TransferLiveAsync(rowId, fromUserId, toUserId, cancellationToken);
        return movedDraft || movedLive;
    }

    public async Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await db.Users
            .AsNoTracking()
            .Where(user => userIds.Contains(user.Id))
            .ToDictionaryAsync(user => user.Id, user => user.DisplayName, cancellationToken);
    }

    private async Task<bool> TransferDraftAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
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

    /// <summary>
    /// Takes the row from whoever is in it and gives it to the new holder's tab on that sheet. With no such
    /// tab the row is simply freed, and theirs to click into.
    /// </summary>
    private async Task<bool> TransferLiveAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        if (live.HolderOf(rowId) is not { } holder || holder.UserId != fromUserId)
        {
            return false;
        }

        live.ReleaseRow(rowId);
        if (presence.FindConnectionOf(holder.SheetId, toUserId) is { } tab)
        {
            var handedOn = holder with { UserId = toUserId, DisplayName = tab.Connection.DisplayName };
            live.TryCheckOut(tab.ConnectionId, handedOn, out _);
        }

        await notifier.CheckoutsChangedAsync(holder.SheetId, live.OnSheet(holder.SheetId), cancellationToken);
        return true;
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
