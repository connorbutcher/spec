using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowCheckouts : IRowCheckouts
{
    private readonly ISheetRowRepository rows;
    private readonly ISheetRowRevisionRepository rowRevisions;
    private readonly IRowValueRepository values;
    private readonly IUserRepository users;
    private readonly IUnitOfWork unitOfWork;
    private readonly LiveRowCheckoutTracker live;
    private readonly SheetPresenceTracker presence;
    private readonly ISheetLiveNotifier notifier;
    private readonly TimeProvider clock;

    public RowCheckouts(
        ISheetRowRepository rows,
        ISheetRowRevisionRepository rowRevisions,
        IRowValueRepository values,
        IUserRepository users,
        IUnitOfWork unitOfWork,
        LiveRowCheckoutTracker live,
        SheetPresenceTracker presence,
        ISheetLiveNotifier notifier,
        TimeProvider clock)
    {
        this.rows = rows;
        this.rowRevisions = rowRevisions;
        this.values = values;
        this.users = users;
        this.unitOfWork = unitOfWork;
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
        return await rowRevisions.FindDraftAuthorAsync(rowId, cancellationToken);
    }

    public async Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await rows.FindSheetIdAsync(rowId, cancellationToken);
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
        return await users.GetDisplayNamesAsync(userIds, cancellationToken);
    }

    /// <summary>The row's checkout only if it is a draft: someone has saved something on it.</summary>
    private async Task<RowCheckout?> FindDraftAsync(int rowId, CancellationToken cancellationToken)
    {
        var revisions = await rowRevisions.ListCurrentAndDraftAsync(rowId, cancellationToken);
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
        var draft = await rowRevisions.FindDraftForEditAsync(rowId, cancellationToken);
        // A draft waiting for approval stays with its author until it is decided.
        if (draft is null || draft.AuthorUserId != fromUserId || draft.SheetSubmissionId is not null)
        {
            return false;
        }

        // The draft's row version makes this fail, as a conflict, if its holder saves at the same moment.
        draft.AuthorUserId = toUserId;
        draft.UpdatedAtUtc = clock.GetUtcNow().UtcDateTime;
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
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

        var held = await values.LoadAsync([draft.Id, published.Id], cancellationToken);
        return !CellValuesComparer.Same(held.GetValueOrDefault(draft.Id), held.GetValueOrDefault(published.Id));
    }
}
