using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

public sealed class SheetPresenceService : ISheetPresenceService
{
    private readonly SheetPresenceTracker _tracker;
    private readonly RowTakeoverStore _takeovers;
    private readonly LiveRowCheckoutTracker _checkouts;
    private readonly ILiveRowCheckoutService _rowCheckouts;
    private readonly ISheetLiveNotifier _notifier;
    private readonly IUserDataService _userData;
    private readonly ISheetDataService _sheetData;

    public SheetPresenceService(
        SheetPresenceTracker tracker,
        RowTakeoverStore takeovers,
        LiveRowCheckoutTracker checkouts,
        ILiveRowCheckoutService rowCheckouts,
        ISheetLiveNotifier notifier,
        IUserDataService userData,
        ISheetDataService sheetData)
    {
        _tracker = tracker;
        _takeovers = takeovers;
        _checkouts = checkouts;
        _rowCheckouts = rowCheckouts;
        _notifier = notifier;
        _userData = userData;
        _sheetData = sheetData;
    }

    public async Task<SheetLiveState> JoinAsync(string connectionId, int sheetId, int userId, CancellationToken cancellationToken)
    {
        if (!await _sheetData.ExistsAsync(sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        var displayName = await _userData.FindActiveDisplayNameAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User {userId} was not found.");

        _tracker.Join(connectionId, new SheetConnection { SheetId = sheetId, UserId = userId, DisplayName = displayName });
        var users = _tracker.UsersOn(sheetId);
        await _notifier.PresenceChangedAsync(sheetId, users, cancellationToken);

        return new SheetLiveState
        {
            Users = users,
            Checkouts = _checkouts.OnSheet(sheetId),
            Takeovers = _takeovers.Involving(sheetId, userId),
        };
    }

    public async Task<SheetConnection?> LeaveAsync(string connectionId, CancellationToken cancellationToken)
    {
        // A tab that has gone is no longer in any row.
        await _rowCheckouts.ReleaseAsync(connectionId, cancellationToken);

        var left = _tracker.Leave(connectionId);
        if (left is not null)
        {
            await _notifier.PresenceChangedAsync(left.SheetId, _tracker.UsersOn(left.SheetId), cancellationToken);
        }

        return left;
    }
}
