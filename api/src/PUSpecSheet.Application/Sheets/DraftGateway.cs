using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Loads an item's current revision and draft, and starts the current user's draft on it. A draft is the
/// item's lock: nobody else can change the item until its author publishes or discards it. One subclass
/// per revision type supplies the queries, which EF needs written against the concrete entity.
/// </summary>
public abstract class DraftGateway<TRevision>(PuSpecSheetDbContext db, ICurrentUser currentUser)
    where TRevision : class, ISheetRevision, new()
{
    protected PuSpecSheetDbContext Db => db;

    /// <summary>Said in the lock message, e.g. "This row".</summary>
    protected abstract string Subject { get; }

    /// <summary>The current published revision and any drafts of the given items, tracked.</summary>
    protected abstract IQueryable<TRevision> CurrentAndDrafts(IReadOnlyCollection<int> itemIds);

    /// <summary>The item a revision belongs to.</summary>
    protected abstract int ItemIdOf(TRevision revision);

    protected abstract Task<int> LastRevisionNumberAsync(int itemId, CancellationToken cancellationToken);

    /// <summary>Points a new revision at its item and adds it to the context.</summary>
    protected abstract void Add(TRevision revision, int itemId);

    /// <summary>
    /// The drafts that make no net difference to the published revision they started from, so they needn't be
    /// kept: the same place in the order and the same existence. Revision types with more to compare add to
    /// this. It takes every pair at once so a type that has to read more (a row's values) reads it in one go.
    /// </summary>
    protected virtual Task<HashSet<TRevision>> UnchangedAsync(
        IReadOnlyList<(TRevision Draft, TRevision Current)> pairs,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(pairs
            .Where(pair => SheetRevisionComparer.SameStructure(pair.Draft, pair.Current))
            .Select(pair => pair.Draft)
            .ToHashSet());
    }

    /// <summary>
    /// Throws the viewer's draft away, releasing the lock, when it has been put back to what's published (a
    /// value typed back, an item moved back to where it was, a title restored). Nothing is left to publish.
    /// An item that has never been published has no original to return to, so it is left alone.
    /// </summary>
    public Task ReleaseIfUnchangedAsync(int itemId, CancellationToken cancellationToken)
    {
        return ReleaseUnchangedAsync([itemId], cancellationToken);
    }

    /// <summary>
    /// <see cref="ReleaseIfUnchangedAsync"/> for many items at once: one read of their revisions and one save,
    /// however many there are.
    /// </summary>
    public Task ReleaseUnchangedAsync(IReadOnlyCollection<int> itemIds, CancellationToken cancellationToken)
    {
        return ReleaseUnchangedAsync(itemIds, everyone: false, cancellationToken);
    }

    /// <summary>
    /// <see cref="ReleaseUnchangedAsync(IReadOnlyCollection{int}, CancellationToken)"/>, taking other people's
    /// drafts as well when <paramref name="everyone"/> is set. That is for publishing everything on a sheet,
    /// where a draft that changes nothing must not become a revision whoever left it.
    /// </summary>
    public async Task ReleaseUnchangedAsync(IReadOnlyCollection<int> itemIds, bool everyone, CancellationToken cancellationToken)
    {
        var states = await LoadManyAsync(itemIds, cancellationToken);
        var drafts = everyone ? DraftsOnPublishedItems(states.Values) : MyDraftsOnPublishedItems(states.Values);
        var unchanged = await UnchangedAsync(drafts, cancellationToken);
        if (unchanged.Count == 0)
        {
            return;
        }

        db.RemoveRange(unchanged);
        await db.SaveSheetChangesAsync(cancellationToken);
    }

    /// <summary>
    /// After items in a group of siblings have been moved, drops the viewer's drafts that only differ in their
    /// numeric order when the group is back in its published order (see <see cref="OrderRestoration"/>). Each
    /// such item goes back to its published number, so it is exactly what is published and nothing is left to
    /// publish. A draft with any other difference (a changed value, a removal) is kept.
    /// </summary>
    public async Task ReleaseRestoredOrderAsync(IReadOnlyCollection<int> siblingIds, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
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
            db.RemoveRange(unchanged);
            await db.SaveSheetChangesAsync(cancellationToken);
        }
    }

    public async Task<DraftState<TRevision>> LoadAsync(int itemId, CancellationToken cancellationToken)
    {
        var states = await LoadManyAsync([itemId], cancellationToken);
        return states.GetValueOrDefault(itemId) ?? throw new NotFoundException($"{Subject} was not found.");
    }

    /// <summary>
    /// The current revision and draft of each of the given items, in one query. An item with neither (it
    /// doesn't exist) is left out.
    /// </summary>
    public async Task<IReadOnlyDictionary<int, DraftState<TRevision>>> LoadManyAsync(
        IReadOnlyCollection<int> itemIds,
        CancellationToken cancellationToken)
    {
        if (itemIds.Count == 0)
        {
            return new Dictionary<int, DraftState<TRevision>>();
        }

        var revisions = await CurrentAndDrafts(itemIds).ToListAsync(cancellationToken);
        return revisions
            .GroupBy(ItemIdOf)
            .ToDictionary(
                item => item.Key,
                item => new DraftState<TRevision>(
                    item.FirstOrDefault(revision => revision.Status == RevisionStatus.Published),
                    item.FirstOrDefault(revision => revision.Status == RevisionStatus.Draft)));
    }

    /// <summary>The viewer's draft on the item, starting one from the current revision if they don't have one yet.</summary>
    /// <exception cref="ConflictException">Someone else holds the item's draft.</exception>
    public async Task<(TRevision Draft, bool Created)> EnsureMineAsync(
        int itemId,
        DraftState<TRevision> state,
        CancellationToken cancellationToken)
    {
        if (state.Draft is { } existing)
        {
            if (existing.AuthorUserId != currentUser.UserId)
            {
                await ThrowLockedAsync(existing.AuthorUserId, cancellationToken);
            }

            existing.UpdatedAtUtc = DateTime.UtcNow;
            return (existing, false);
        }

        var now = DateTime.UtcNow;
        var draft = new TRevision
        {
            RevisionNumber = await LastRevisionNumberAsync(itemId, cancellationToken) + 1,
            Status = RevisionStatus.Draft,
            DisplayOrder = state.Current?.DisplayOrder ?? 0,
            IsDeleted = state.Current?.IsDeleted ?? false,
            AuthorUserId = currentUser.UserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
        };
        Add(draft, itemId);
        return (draft, true);
    }

    /// <summary>Refuses when someone else holds the item's draft; does nothing for no draft or the viewer's own.</summary>
    public async Task EnsureNotLockedByOthersAsync(DraftState<TRevision> state, CancellationToken cancellationToken)
    {
        if (state.Draft is { } draft && draft.AuthorUserId != currentUser.UserId)
        {
            await ThrowLockedAsync(draft.AuthorUserId, cancellationToken);
        }
    }

    /// <summary>The viewer's drafts on items that have a published revision to compare them with.</summary>
    private List<(TRevision Draft, TRevision Current)> MyDraftsOnPublishedItems(IEnumerable<DraftState<TRevision>> states)
    {
        var me = currentUser.UserId;
        return DraftsOnPublishedItems(states.Where(state => state.Draft?.AuthorUserId == me));
    }

    /// <summary>Anyone's drafts on items that have a published revision to compare them with.</summary>
    private static List<(TRevision Draft, TRevision Current)> DraftsOnPublishedItems(IEnumerable<DraftState<TRevision>> states)
    {
        return states
            .Where(state => state.Draft is not null && state.Current is not null)
            .Select(state => (state.Draft!, state.Current!))
            .ToList();
    }

    private async Task ThrowLockedAsync(int authorUserId, CancellationToken cancellationToken)
    {
        var name = await db.Users
            .Where(user => user.Id == authorUserId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken) ?? "another user";
        throw new ConflictException($"{Subject} is being edited by {name}.");
    }
}
