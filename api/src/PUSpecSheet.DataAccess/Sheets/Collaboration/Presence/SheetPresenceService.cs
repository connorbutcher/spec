using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Users.Queries;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;

public sealed class SheetPresenceService : ISheetPresenceService
{
    private readonly PuSpecSheetDbContext _db;
    private readonly SheetPresenceTracker _tracker;
    private readonly RowTakeoverStore _takeovers;
    private readonly LiveRowCheckoutTracker _checkouts;
    private readonly ILiveRowCheckoutService _rowCheckouts;
    private readonly ISheetLiveNotifier _notifier;

    public SheetPresenceService(
        PuSpecSheetDbContext db,
        SheetPresenceTracker tracker,
        RowTakeoverStore takeovers,
        LiveRowCheckoutTracker checkouts,
        ILiveRowCheckoutService rowCheckouts,
        ISheetLiveNotifier notifier)
    {
        _db = db;
        _tracker = tracker;
        _takeovers = takeovers;
        _checkouts = checkouts;
        _rowCheckouts = rowCheckouts;
        _notifier = notifier;
    }

    public async Task<SheetLiveStateDto> JoinAsync(string connectionId, int sheetId, int userId, CancellationToken cancellationToken)
    {
        if (!await SheetQueries.ExistsAsync(_db, sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        var displayName = await UserQueries.FindActiveDisplayNameAsync(_db, userId, cancellationToken)
            ?? throw new NotFoundException($"User {userId} was not found.");

        _tracker.Join(connectionId, new SheetConnection { SheetId = sheetId, UserId = userId, DisplayName = displayName });
        var users = _tracker.UsersOn(sheetId);
        await _notifier.PresenceChangedAsync(sheetId, users, cancellationToken);

        return new SheetLiveStateDto
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
