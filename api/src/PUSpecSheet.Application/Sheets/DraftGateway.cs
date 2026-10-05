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

    /// <summary>The item's current published revision and any drafts, tracked.</summary>
    protected abstract IQueryable<TRevision> CurrentAndDrafts(int itemId);

    protected abstract Task<int> LastRevisionNumberAsync(int itemId, CancellationToken cancellationToken);

    /// <summary>Points a new revision at its item and adds it to the context.</summary>
    protected abstract void Add(TRevision revision, int itemId);

    /// <summary>
    /// Whether a draft makes no net difference to the published revision it started from, so it needn't be kept:
    /// the same place in the order and the same existence. Revision types with more to compare add to this.
    /// </summary>
    protected virtual Task<bool> IsUnchangedAsync(TRevision draft, TRevision current, CancellationToken cancellationToken)
    {
        return Task.FromResult(SheetRevisionComparer.SameStructure(draft, current));
    }

    /// <summary>
    /// Throws the viewer's draft away, releasing the lock, when it has been put back to what's published (a
    /// value typed back, an item moved back to where it was, a title restored). Nothing is left to publish.
    /// An item that has never been published has no original to return to, so it is left alone.
    /// </summary>
    /// <returns>True when a draft was dropped.</returns>
    public async Task<bool> ReleaseIfUnchangedAsync(int itemId, CancellationToken cancellationToken)
    {
        var state = await LoadAsync(itemId, cancellationToken);
        if (state.Draft is not { } draft || state.Current is not { } current || draft.AuthorUserId != currentUser.UserId)
        {
            return false;
        }

        if (!await IsUnchangedAsync(draft, current, cancellationToken))
        {
            return false;
        }

        db.Remove(draft);
        await db.SaveSheetChangesAsync(cancellationToken);
        return true;
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
        var states = new List<(int Id, DraftState<TRevision> State)>();
        foreach (var id in siblingIds)
        {
            states.Add((id, await LoadAsync(id, cancellationToken)));
        }

        var items = states
            .Select(entry =>
            {
                var shown = entry.State.Draft is { } draft && draft.AuthorUserId == me ? draft : entry.State.Current;
                return (entry.Id, entry.State.Current?.DisplayOrder, shown?.DisplayOrder ?? 0, shown?.IsDeleted ?? true);
            })
            .ToList();
        if (!OrderRestoration.IsRestored(items))
        {
            return;
        }

        var released = false;
        foreach (var (_, state) in states)
        {
            if (state.Draft is not { } draft || state.Current is not { } current || draft.AuthorUserId != me
                || draft.DisplayOrder == current.DisplayOrder)
            {
                continue;
            }

            var movedBack = draft.DisplayOrder;
            draft.DisplayOrder = current.DisplayOrder;
            if (await IsUnchangedAsync(draft, current, cancellationToken))
            {
                db.Remove(draft);
                released = true;
            }
            else
            {
                draft.DisplayOrder = movedBack;
            }
        }

        if (released)
        {
            await db.SaveSheetChangesAsync(cancellationToken);
        }
    }

    public async Task<DraftState<TRevision>> LoadAsync(int itemId, CancellationToken cancellationToken)
    {
        var revisions = await CurrentAndDrafts(itemId).ToListAsync(cancellationToken);
        var draft = revisions.FirstOrDefault(revision => revision.Status == RevisionStatus.Draft);
        var current = revisions.FirstOrDefault(revision => revision.Status == RevisionStatus.Published);

        if (draft is null && current is null)
        {
            throw new NotFoundException($"{Subject} was not found.");
        }

        return new DraftState<TRevision>(current, draft);
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

    private async Task ThrowLockedAsync(int authorUserId, CancellationToken cancellationToken)
    {
        var name = await db.Users
            .Where(user => user.Id == authorUserId)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken) ?? "another user";
        throw new ConflictException($"{Subject} is being edited by {name}.");
    }
}
