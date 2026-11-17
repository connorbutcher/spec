using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

public sealed class RowCheckouts : IRowCheckouts
{
    private readonly PuSpecSheetDbContext _db;
    private readonly LiveRowCheckoutTracker _live;
    private readonly SheetPresenceTracker _presence;
    private readonly ISheetLiveNotifier _notifier;
    private readonly TimeProvider _clock;
    private readonly IUserDataService _userData;

    public RowCheckouts(
        PuSpecSheetDbContext db,
        LiveRowCheckoutTracker live,
        SheetPresenceTracker presence,
        ISheetLiveNotifier notifier,
        TimeProvider clock,
        IUserDataService userData)
    {
        _db = db;
        _live = live;
        _presence = presence;
        _notifier = notifier;
        _clock = clock;
        _userData = userData;
    }

    public async Task<RowCheckoutState?> FindAsync(int rowId, CancellationToken cancellationToken)
    {
        if (await FindDraftAsync(rowId, cancellationToken) is { } drafted)
        {
            return drafted;
        }

        // Someone who is only in the row has, by definition, changed nothing.
        return _live.HolderOf(rowId) is { } holder
            ? new RowCheckoutState { SheetId = holder.SheetId, HolderUserId = holder.UserId, HasChanges = false }
            : null;
    }

    public async Task<int?> DraftHolderAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowRevisionQueries.FindDraftAuthorAsync(_db, rowId, cancellationToken);
    }

    public async Task<int?> SheetOfAsync(int rowId, CancellationToken cancellationToken)
    {
        return await SheetRowQueries.FindSheetIdAsync(_db, rowId, cancellationToken);
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
        return await _userData.GetDisplayNamesAsync(userIds, cancellationToken);
    }

    /// <summary>The row's checkout only if it is a draft: someone has saved something on it.</summary>
    private async Task<RowCheckoutState?> FindDraftAsync(int rowId, CancellationToken cancellationToken)
    {
        var revisions = await SheetRowRevisionQueries.ListCurrentAndDraftAsync(_db, rowId, cancellationToken);
        var draft = revisions.SingleOrDefault(revision => revision.Status == RevisionStatus.Draft);
        if (draft is null)
        {
            return null;
        }

        // The draft carries its sheet, so finding the sheet takes no query of its own.
        var published = revisions.SingleOrDefault(revision => revision.Status == RevisionStatus.Published);
        return new RowCheckoutState
        {
            SheetId = draft.SheetId,
            HolderUserId = draft.AuthorUserId,
            HasChanges = await HasChangesAsync(draft, published, cancellationToken),
        };
    }

    private async Task<bool> TransferDraftAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        var draft = await SheetRowRevisionQueries.FindDraftForEditAsync(_db, rowId, cancellationToken);
        // A draft waiting for approval stays with its author until it is decided.
        if (draft is null || draft.AuthorUserId != fromUserId || draft.SheetSubmissionId is not null)
        {
            return false;
        }

        // The draft's row version makes this fail, as a conflict, if its holder saves at the same moment.
        draft.AuthorUserId = toUserId;
        draft.UpdatedAtUtc = _clock.GetUtcNow().UtcDateTime;
        await _db.SaveSheetChangesAsync(cancellationToken);
        return true;
    }

    /// <summary>
    /// Takes the row from whoever is in it and gives it to the new holder's tab on that sheet. With no such
    /// tab the row is simply freed, and theirs to click into.
    /// </summary>
    private async Task<bool> TransferLiveAsync(int rowId, int fromUserId, int toUserId, CancellationToken cancellationToken)
    {
        if (_live.HolderOf(rowId) is not { } holder || holder.UserId != fromUserId)
        {
            return false;
        }

        _live.ReleaseRow(rowId);
        if (_presence.FindConnectionOf(holder.SheetId, toUserId) is { } tab)
        {
            var handedOn = holder with { UserId = toUserId, DisplayName = tab.Connection.DisplayName };
            _live.TryCheckOut(tab.ConnectionId, handedOn, out _);
        }

        await _notifier.CheckoutsChangedAsync(holder.SheetId, _live.OnSheet(holder.SheetId), cancellationToken);
        return true;
    }

    /// <summary>
    /// Whether a draft differs from what is published, by the same comparison that decides whether a draft
    /// is worth keeping (<see cref="RowDrafts"/>): its place, its existence and every cell's value. A row
    /// that has never been published is all change.
    /// </summary>
    private async Task<bool> HasChangesAsync(DbSheetRowRevision draft, DbSheetRowRevision? published, CancellationToken cancellationToken)
    {
        if (published is null || !SheetRevisionComparer.SameStructure(draft, published))
        {
            return true;
        }

        var held = await RowValueQueries.LoadAsync(_db, [draft.Id, published.Id], cancellationToken);
        return !CellValuesComparer.Same(held.GetValueOrDefault(draft.Id), held.GetValueOrDefault(published.Id));
    }
}
