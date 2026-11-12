using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class SheetPresenceService : ISheetPresenceService
{
    private readonly ISheetRepository sheets;
    private readonly IUserRepository userRepository;
    private readonly SheetPresenceTracker tracker;
    private readonly RowTakeoverStore takeovers;
    private readonly LiveRowCheckoutTracker checkouts;
    private readonly ILiveRowCheckoutService rowCheckouts;
    private readonly ISheetLiveNotifier notifier;

    public SheetPresenceService(
        ISheetRepository sheets,
        IUserRepository userRepository,
        SheetPresenceTracker tracker,
        RowTakeoverStore takeovers,
        LiveRowCheckoutTracker checkouts,
        ILiveRowCheckoutService rowCheckouts,
        ISheetLiveNotifier notifier)
    {
        this.sheets = sheets;
        this.userRepository = userRepository;
        this.tracker = tracker;
        this.takeovers = takeovers;
        this.checkouts = checkouts;
        this.rowCheckouts = rowCheckouts;
        this.notifier = notifier;
    }

    public async Task<SheetLiveStateDto> JoinAsync(string connectionId, int sheetId, int userId, CancellationToken cancellationToken)
    {
        if (!await sheets.ExistsAsync(sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        var displayName = await userRepository.FindActiveDisplayNameAsync(userId, cancellationToken)
            ?? throw new NotFoundException($"User {userId} was not found.");

        tracker.Join(connectionId, new SheetConnection(sheetId, userId, displayName));
        var users = tracker.UsersOn(sheetId);
        await notifier.PresenceChangedAsync(sheetId, users, cancellationToken);

        return new SheetLiveStateDto(users, checkouts.OnSheet(sheetId), takeovers.Involving(sheetId, userId));
    }

    public async Task<SheetConnection?> LeaveAsync(string connectionId, CancellationToken cancellationToken)
    {
        // A tab that has gone is no longer in any row.
        await rowCheckouts.ReleaseAsync(connectionId, cancellationToken);

        var left = tracker.Leave(connectionId);
        if (left is not null)
        {
            await notifier.PresenceChangedAsync(left.SheetId, tracker.UsersOn(left.SheetId), cancellationToken);
        }

        return left;
    }
}
