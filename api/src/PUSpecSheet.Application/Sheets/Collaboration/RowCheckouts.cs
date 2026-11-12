using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.Users.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowCheckouts : IRowCheckouts
{
    private readonly PuSpecSheetDbContext db;
    private readonly LiveRowCheckoutTracker live;
    private readonly SheetPresenceTracker presence;
    private readonly ISheetLiveNotifier notifier;
    private readonly TimeProvider clock;

    public RowCheckouts(
        PuSpecSheetDbContext db,
        LiveRowCheckoutTracker live,
        SheetPresenceTracker presence,
        ISheetLiveNotifier notifier,
        TimeProvider clock)
    {
        this.db = db;
        this.live = live;
        this.presence = presence;
        this.notifier = notifier;
        this.clock = clock;
    }

    public async Task<RowCheckout?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        if (await FindDraftAsync(rowId, cancellationToken) is { } drafted)
        {
            return drafted;
        }

        // Someone who is only in the row has, by definition, changed nothing.
        return live.HolderOf(rowId) is { } holder
            ? new RowCheckout(holder.SheetId, holder.UserId, hasChanges: false)
            : null;
    }

    public async Task<int?> DraftHolderAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowRevisionQueries.FindDraftAuthorAsync(db, rowId, cancellationToken);
    }

    public async Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowQueries.FindSheetIdAsync(db, rowId, cancellationToken);
    }

    public async Task<RowTransfer> TransferAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        var movedDraft = await TransferDraftAsync(rowId, fromUserId, toUserId, cancellationToken);
        var movedLive = await TransferLiveAsync(rowId, fromUserId, toUserId, cancellationToken);
        if (movedDraft)
        {
            return RowTransfer.Draft;
        }

        return movedLive ? RowTransfer.Live : RowTransfer.Nothing;
    }

    public async Task<IReadOnlyDictionary<int, string>> DisplayNamesAsync(IReadOnlyCollection<int> userIds, CancellationToken cancellationToken)
    {
        return await UserQueries.GetDisplayNamesAsync(db, userIds, cancellationToken);
    }

    /// <summary>The row's checkout only if it is a draft: someone has saved something on it.</summary>
    private async Task<RowCheckout?> FindDraftAsync(int rowId, CancellationToken cancellationToken)
    {
        var revisions = await SheetRowRevisionQueries.ListCurrentAndDraftAsync(db, rowId, cancellationToken);
        var draft = revisions.SingleOrDefault(revision => revision.Status == RevisionStatus.Draft);
        if (draft is null)
        {
            return null;
        }

        // The draft carries its sheet, so finding the sheet takes no query of its own.
        var published = revisions.SingleOrDefault(revision => revision.Status == RevisionStatus.Published);
        return new RowCheckout(draft.SheetId, draft.AuthorUserId, await HasChangesAsync(draft, published, cancellationToken));
    }

    private async Task<bool> TransferDraftAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        var draft = await SheetRowRevisionQueries.FindDraftForEditAsync(db, rowId, cancellationToken);
        // A draft waiting for approval stays with its author until it is decided.
        if (draft is null || draft.AuthorUserId != fromUserId || draft.SheetSubmissionId is not null)
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

        var held = await RowValueQueries.LoadAsync(db, [draft.Id, published.Id], cancellationToken);
        return !CellValuesComparer.Same(held.GetValueOrDefault(draft.Id), held.GetValueOrDefault(published.Id));
    }
}
