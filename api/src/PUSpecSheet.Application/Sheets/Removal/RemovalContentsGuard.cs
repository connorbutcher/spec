using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Application.Users;

namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>
/// Stops a section or table being removed while someone else holds something inside it, or anything inside
/// is waiting for approval (see <see cref="HeldContents"/>). Both kinds of hold count: a saved draft, and a
/// row someone has only clicked into.
/// </summary>
public sealed class RemovalContentsGuard
{
    private readonly SheetRevisionRepositories revisions;
    private readonly ISheetRowRepository rows;
    private readonly IUserRepository users;
    private readonly LiveRowCheckoutTracker liveCheckouts;
    private readonly ICurrentUser currentUser;

    public RemovalContentsGuard(
        SheetRevisionRepositories revisions,
        ISheetRowRepository rows,
        IUserRepository users,
        LiveRowCheckoutTracker liveCheckouts,
        ICurrentUser currentUser)
    {
        this.revisions = revisions;
        this.rows = rows;
        this.users = users;
        this.liveCheckouts = liveCheckouts;
        this.currentUser = currentUser;
    }

    /// <summary>Refuses removing the sections given (a section and everything inside it) while they are held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureSectionsFreeAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await revisions.Sections.ListHoldsAsync(sectionIds, currentUser.UserId, cancellationToken));
        holds.AddRange(await revisions.Rows.ListHoldsInSectionsAsync(sectionIds, currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await rows.ListIdsInSectionsAsync(sectionIds, cancellationToken)));
        await ThrowIfHeldAsync("this section", holds, cancellationToken);
    }

    /// <summary>Refuses removing a table while anything in it is held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureTableFreeAsync(int tableId, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await revisions.Sections.ListHoldsOnTableAsync(tableId, currentUser.UserId, cancellationToken));
        holds.AddRange(await revisions.Rows.ListHoldsOnTableAsync(tableId, currentUser.UserId, cancellationToken));
        holds.AddRange(await revisions.ColumnBlocks.ListHoldsOnTableAsync(tableId, currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await rows.ListIdsOnTableAsync(tableId, cancellationToken)));
        await ThrowIfHeldAsync("this table", holds, cancellationToken);
    }

    /// <summary>The rows among <paramref name="rowIds"/> that someone has clicked into, which only memory knows about.</summary>
    private List<ContainedHold> LiveHolds(List<int> rowIds)
    {
        return rowIds
            .Select(liveCheckouts.HolderOf)
            .OfType<LiveRowCheckout>()
            .Select(holder => new ContainedHold(holder.UserId, awaitingApproval: false))
            .ToList();
    }

    private async Task ThrowIfHeldAsync(string subject, List<ContainedHold> holds, CancellationToken cancellationToken)
    {
        var userIds = holds.Select(hold => hold.UserId).Distinct().ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var names = await users.GetDisplayNamesAsync(userIds, cancellationToken);
        if (HeldContents.RefusalFor(subject, holds, currentUser.UserId, names) is { } refusal)
        {
            throw new ConflictException(refusal);
        }
    }
}
