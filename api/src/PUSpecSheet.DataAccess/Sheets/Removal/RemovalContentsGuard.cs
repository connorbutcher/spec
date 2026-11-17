using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.DataAccess.Users.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Sheets.Removal;

/// <summary>
/// Stops a section or table being removed while someone else holds something inside it, or anything inside
/// is waiting for approval (see <see cref="HeldContents"/>). Both kinds of hold count: a saved draft, and a
/// row someone has only clicked into.
/// </summary>
public sealed class RemovalContentsGuard
{
    private readonly PuSpecSheetDbContext _db;
    private readonly LiveRowCheckoutTracker _liveCheckouts;
    private readonly ICurrentUser _currentUser;

    public RemovalContentsGuard(
        PuSpecSheetDbContext db,
        LiveRowCheckoutTracker liveCheckouts,
        ICurrentUser currentUser)
    {
        _db = db;
        _liveCheckouts = liveCheckouts;
        _currentUser = currentUser;
    }

    /// <summary>Refuses removing the sections given (a section and everything inside it) while they are held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureSectionsFreeAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await SheetSectionRevisionQueries.ListHoldsAsync(_db, sectionIds, _currentUser.UserId, cancellationToken));
        holds.AddRange(await SheetRowRevisionQueries.ListHoldsInSectionsAsync(_db, sectionIds, _currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await SheetRowQueries.ListIdsInSectionsAsync(_db, sectionIds, cancellationToken)));
        await ThrowIfHeldAsync("this section", holds, cancellationToken);
    }

    /// <summary>Refuses removing a table while anything in it is held.</summary>
    /// <exception cref="ConflictException">Someone else holds something inside, or it is waiting for approval.</exception>
    public async Task EnsureTableFreeAsync(int tableId, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await SheetSectionRevisionQueries.ListHoldsOnTableAsync(_db, tableId, _currentUser.UserId, cancellationToken));
        holds.AddRange(await SheetRowRevisionQueries.ListHoldsOnTableAsync(_db, tableId, _currentUser.UserId, cancellationToken));
        holds.AddRange(await SheetColumnBlockRevisionQueries.ListHoldsOnTableAsync(_db, tableId, _currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await SheetRowQueries.ListIdsOnTableAsync(_db, tableId, cancellationToken)));
        await ThrowIfHeldAsync("this table", holds, cancellationToken);
    }

    /// <summary>The rows among <paramref name="rowIds"/> that someone has clicked into, which only memory knows about.</summary>
    private List<ContainedHold> LiveHolds(List<int> rowIds)
    {
        return rowIds
            .Select(_liveCheckouts.HolderOf)
            .OfType<LiveRowCheckout>()
            .Select(holder => new ContainedHold { UserId = holder.UserId, AwaitingApproval = false })
            .ToList();
    }

    private async Task ThrowIfHeldAsync(string subject, List<ContainedHold> holds, CancellationToken cancellationToken)
    {
        var userIds = holds.Select(hold => hold.UserId).Distinct().ToList();
        if (userIds.Count == 0)
        {
            return;
        }

        var names = await UserQueries.GetDisplayNamesAsync(_db, userIds, cancellationToken);
        if (HeldContents.RefusalFor(subject, holds, _currentUser.UserId, names) is { } refusal)
        {
            throw new ConflictException(refusal);
        }
    }
}
