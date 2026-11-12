using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Application.Users.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Removal;

/// <summary>
/// Stops a section or table being removed while someone else holds something inside it, or anything inside
/// is waiting for approval (see <see cref="HeldContents"/>). Both kinds of hold count: a saved draft, and a
/// row someone has only clicked into.
/// </summary>
public sealed class RemovalContentsGuard
{
    private readonly PuSpecSheetDbContext db;
    private readonly LiveRowCheckoutTracker liveCheckouts;
    private readonly ICurrentUser currentUser;

    public RemovalContentsGuard(
        PuSpecSheetDbContext db,
        LiveRowCheckoutTracker liveCheckouts,
        ICurrentUser currentUser)
    {
        this.db = db;
        this.liveCheckouts = liveCheckouts;
        this.currentUser = currentUser;
    }

    /// <summary>Refuses removing the sections given (a section and everything inside it) while they are held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureSectionsFreeAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await SheetSectionRevisionQueries.ListHoldsAsync(db, sectionIds, currentUser.UserId, cancellationToken));
        holds.AddRange(await SheetRowRevisionQueries.ListHoldsInSectionsAsync(db, sectionIds, currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await SheetRowQueries.ListIdsInSectionsAsync(db, sectionIds, cancellationToken)));
        await ThrowIfHeldAsync("this section", holds, cancellationToken);
    }

    /// <summary>Refuses removing a table while anything in it is held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureTableFreeAsync(int tableId, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await SheetSectionRevisionQueries.ListHoldsOnTableAsync(db, tableId, currentUser.UserId, cancellationToken));
        holds.AddRange(await SheetRowRevisionQueries.ListHoldsOnTableAsync(db, tableId, currentUser.UserId, cancellationToken));
        holds.AddRange(await SheetColumnBlockRevisionQueries.ListHoldsOnTableAsync(db, tableId, currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await SheetRowQueries.ListIdsOnTableAsync(db, tableId, cancellationToken)));
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

        var names = await UserQueries.GetDisplayNamesAsync(db, userIds, cancellationToken);
        if (HeldContents.RefusalFor(subject, holds, currentUser.UserId, names) is { } refusal)
        {
            throw new ConflictException(refusal);
        }
    }
}
