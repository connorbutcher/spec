using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

public abstract class DraftGateway<TRevision>
    where TRevision : class, IDbSheetRevision, new()
{
    private readonly PuSpecSheetDbContext _db;
    private readonly ICurrentUser _currentUser;
    private readonly IUserDataService _userData;

    protected DraftGateway(
        PuSpecSheetDbContext db,
        ICurrentUser currentUser,
        IUserDataService userData)
    {
        _db = db;
        _currentUser = currentUser;
        _userData = userData;
    }

    protected PuSpecSheetDbContext Db => _db;

    protected abstract DbSet<TRevision> Revisions { get; }

    protected abstract string Subject { get; }

    protected abstract int ItemIdOf(TRevision revision);

    protected abstract IQueryable<TRevision> OfItems(IReadOnlyCollection<int> itemIds);

    protected abstract IQueryable<TRevision> OfItem(int itemId);

    protected abstract void SetItem(TRevision revision, int itemId);

    // What a new draft carries over from the current revision besides its order and removal.
    protected virtual void CopyContent(TRevision current, TRevision draft)
    {
    }

    // The drafts that make no net difference to the published revision they started from, so they needn't be kept:
    // the same place in the order and the same existence. Revision types with more to compare add to this. It takes
    // every pair at once so a type that has to read more (a row's values) reads it in one go.
    protected virtual Task<HashSet<TRevision>> UnchangedAsync(
        IReadOnlyList<(TRevision Draft, TRevision Current)> pairs,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(pairs
            .Where(pair => SheetRevisionComparer.SameStructure(pair.Draft, pair.Current))
            .Select(pair => pair.Draft)
            .ToHashSet());
    }

    // Throws the viewer's draft away, releasing the lock, when it has been put back to what's published (a value
    // typed back, an item moved back to where it was, a title restored). Nothing is left to publish. An item that has
    // never been published has no original to return to, so it is left alone.
    public Task ReleaseIfUnchangedAsync(int itemId, CancellationToken cancellationToken)
    {
        return ReleaseUnchangedAsync([itemId], cancellationToken);
    }

    public Task ReleaseUnchangedAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken)
    {
        return ReleaseUnchangedAsync(itemIds, everyone: false, cancellationToken);
    }

    // ReleaseUnchangedAsync, taking other people's drafts as well when everyone is set. That is for publishing
    // everything on a sheet, where a draft that changes nothing must not become a revision whoever left it.
    public async Task ReleaseUnchangedAsync(IReadOnlyCollection<int> itemIds, bool everyone, CancellationToken cancellationToken)
    {
        var states = await LoadManyAsync(itemIds, cancellationToken);
        var drafts = everyone ? DraftsOnPublishedItems(states.Values) : MyDraftsOnPublishedItems(states.Values);
        var unchanged = await UnchangedAsync(drafts, cancellationToken);
        if (unchanged.Count == 0)
        {
            return;
        }

        Revisions.RemoveRange(unchanged);
        await _db.SaveSheetChangesAsync(cancellationToken);
    }

    // After items in a group of siblings have been moved, drops the viewer's drafts that only differ in their numeric
    // order when the group is back in its published order (see OrderRestoration). Each such item goes back to its
    // published number, so it is exactly what is published and nothing is left to publish. A draft with any other
    // difference (a changed value, a removal) is kept.
    public async Task ReleaseRestoredOrderAsync(IReadOnlyCollection<int> siblingIds, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var states = await LoadManyAsync(siblingIds, cancellationToken);
        var items = states
            .Select(entry =>
            {
                var shown = entry.Value.Draft is { } draft && draft.AuthorUserId == me ? draft : entry.Value.Current;
                return (entry.Key, entry.Value.Current?.DisplayOrder, shown?.DisplayOrder ?? 0, shown?.IsDeleted ?? true);
            })
            .ToList();
        if (!OrderRestoration.IsRestored(items))
        {
            return;
        }

        var moved = MyDraftsOnPublishedItems(states.Values)
            .Where(pair => pair.Draft.DisplayOrder != pair.Current.DisplayOrder)
            .ToList();

        // Each goes back to its published number to see whether anything else still differs; the ones that do
        // differ keep the number they were moved to.
        var movedTo = moved.ToDictionary(pair => pair.Draft, pair => pair.Draft.DisplayOrder);
        foreach (var (draft, current) in moved)
        {
            draft.DisplayOrder = current.DisplayOrder;
        }

        var unchanged = await UnchangedAsync(moved, cancellationToken);
        foreach (var (draft, _) in moved.Where(pair => !unchanged.Contains(pair.Draft)))
        {
            draft.DisplayOrder = movedTo[draft];
        }

        if (unchanged.Count > 0)
        {
            Revisions.RemoveRange(unchanged);
            await _db.SaveSheetChangesAsync(cancellationToken);
        }
    }

    public async Task<DraftState<TRevision>> LoadAsync(int itemId, CancellationToken cancellationToken)
    {
        var states = await LoadManyAsync([itemId], cancellationToken);
        return states.GetValueOrDefault(itemId) ?? throw new NotFoundException($"{Subject} was not found.");
    }

    public async Task<IReadOnlyDictionary<int, DraftState<TRevision>>> LoadManyAsync(
        IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0)
        {
            return new Dictionary<int, DraftState<TRevision>>();
        }

        var loaded = await OfItems(itemIds).ListCurrentAndDraftsForEditAsync(cancellationToken);
        return loaded
            .GroupBy(ItemIdOf)
            .ToDictionary(
                item => item.Key,
                item => new DraftState<TRevision>
                {
                    Current = item.FirstOrDefault(revision => revision.Status == RevisionStatus.Published),
                    Draft = item.FirstOrDefault(revision => revision.Status == RevisionStatus.Draft),
                });
    }

    public async Task<(TRevision Draft, bool Created)> EnsureMineAsync(
        int itemId,
        DraftState<TRevision> state,
        CancellationToken cancellationToken)
    {
        if (state.Draft is { } existing)
        {
            EnsureNotAwaitingApproval(existing);
            if (existing.AuthorUserId != _currentUser.UserId)
            {
                await ThrowLockedAsync(existing.AuthorUserId, cancellationToken);
            }

            existing.UpdatedAtUtc = DateTime.UtcNow;
            return (existing, false);
        }

        // Every item gets its first revision when it is created, so with no draft there is always a current one.
        var current = state.Current
            ?? throw new InvalidOperationException($"{Subject} has no revision to start a draft from.");
        var now = DateTime.UtcNow;
        var draft = new TRevision
        {
            SheetId = current.SheetId,
            RevisionNumber = await OfItem(itemId).GetLastRevisionNumberAsync(cancellationToken) + 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = current.DisplayOrder,
            IsDeleted = current.IsDeleted,
            AuthorUserId = _currentUser.UserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        SetItem(draft, itemId);
        CopyContent(current, draft);
        Revisions.Add(draft);
        return (draft, true);
    }

    public async Task EnsureNotLockedByOthersAsync(DraftState<TRevision> state, CancellationToken cancellationToken)
    {
        if (state.Draft is { } submitted)
        {
            EnsureNotAwaitingApproval(submitted);
        }

        if (state.Draft is { } draft && draft.AuthorUserId != _currentUser.UserId)
        {
            await ThrowLockedAsync(draft.AuthorUserId, cancellationToken);
        }
    }

    private List<(TRevision Draft, TRevision Current)> MyDraftsOnPublishedItems(IEnumerable<DraftState<TRevision>> states)
    {
        var me = _currentUser.UserId;
        return DraftsOnPublishedItems(states.Where(state => state.Draft?.AuthorUserId == me));
    }

    private static List<(TRevision Draft, TRevision Current)> DraftsOnPublishedItems(IEnumerable<DraftState<TRevision>> states)
    {
        return states
            .Where(state => state.Draft is { SheetSubmissionId: null } && state.Current is not null)
            .Select(state => (state.Draft!, state.Current!))
            .ToList();
    }

    private void EnsureNotAwaitingApproval(TRevision draft)
    {
        if (draft.SheetSubmissionId is not null)
        {
            throw new ConflictException($"{Subject} has changes waiting for approval. It can be changed again once they are approved or rejected.");
        }
    }

    private async Task ThrowLockedAsync(int authorUserId, CancellationToken cancellationToken)
    {
        var name = await _userData.FindDisplayNameAsync(authorUserId, cancellationToken) ?? "another user";
        throw new ConflictException($"{Subject} is being edited by {name}.");
    }
}
