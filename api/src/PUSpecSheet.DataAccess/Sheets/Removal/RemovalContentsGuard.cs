using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Sheets.Removal;

public sealed class RemovalContentsGuard
{
    private readonly LiveRowCheckoutTracker _liveCheckouts;
    private readonly ICurrentUser _currentUser;
    private readonly IUserDataService _userData;
    private readonly ISheetSectionDataService _sheetSectionData;
    private readonly ISheetRowDataService _sheetRowData;
    private readonly ISheetColumnBlockDataService _sheetColumnBlockData;

    public RemovalContentsGuard(
        LiveRowCheckoutTracker liveCheckouts,
        ICurrentUser currentUser,
        IUserDataService userData,
        ISheetSectionDataService sheetSectionData,
        ISheetRowDataService sheetRowData,
        ISheetColumnBlockDataService sheetColumnBlockData)
    {
        _liveCheckouts = liveCheckouts;
        _currentUser = currentUser;
        _userData = userData;
        _sheetSectionData = sheetSectionData;
        _sheetRowData = sheetRowData;
        _sheetColumnBlockData = sheetColumnBlockData;
    }

    public async Task EnsureSectionsFreeAsync(IReadOnlyCollection<int> sectionIds, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await _sheetSectionData.ListHoldsAsync(sectionIds, _currentUser.UserId, cancellationToken));
        holds.AddRange(await _sheetRowData.ListHoldsInSectionsAsync(sectionIds, _currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await _sheetRowData.ListIdsInSectionsAsync(sectionIds, cancellationToken)));
        await ThrowIfHeldAsync("this section", holds, cancellationToken);
    }

    public async Task EnsureTableFreeAsync(int tableId, CancellationToken cancellationToken)
    {
        var holds = new List<ContainedHold>();
        holds.AddRange(await _sheetSectionData.ListHoldsOnTableAsync(tableId, _currentUser.UserId, cancellationToken));
        holds.AddRange(await _sheetRowData.ListHoldsOnTableAsync(tableId, _currentUser.UserId, cancellationToken));
        holds.AddRange(await _sheetColumnBlockData.ListHoldsOnTableAsync(tableId, _currentUser.UserId, cancellationToken));
        holds.AddRange(LiveHolds(await _sheetRowData.ListIdsOnTableAsync(tableId, cancellationToken)));
        await ThrowIfHeldAsync("this table", holds, cancellationToken);
    }

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

        var names = await _userData.GetDisplayNamesAsync(userIds, cancellationToken);
        if (HeldContents.RefusalFor(subject, holds, _currentUser.UserId, names) is { } refusal)
        {
            throw new ConflictException(refusal);
        }
    }
}
