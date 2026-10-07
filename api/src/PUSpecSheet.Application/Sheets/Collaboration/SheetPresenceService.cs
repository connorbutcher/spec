using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Sheets.Collaboration;
using PUSpecSheet.Data;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class SheetPresenceService(
    PuSpecSheetDbContext db,
    SheetPresenceTracker tracker,
    RowTakeoverStore takeovers,
    ISheetLiveNotifier notifier) : ISheetPresenceService
{
    public async Task<SheetLiveStateDto> JoinAsync(string connectionId, int sheetId, int userId, CancellationToken cancellationToken)
    {
        if (!await db.Sheets.AnyAsync(sheet => sheet.Id == sheetId, cancellationToken))
        {
            throw new NotFoundException($"Sheet {sheetId} was not found.");
        }

        var displayName = await db.Users
            .Where(user => user.Id == userId && user.IsActive)
            .Select(user => user.DisplayName)
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"User {userId} was not found.");

        tracker.Join(connectionId, new SheetConnection(sheetId, userId, displayName));
        var users = tracker.UsersOn(sheetId);
        await notifier.PresenceChangedAsync(sheetId, users, cancellationToken);

        return new SheetLiveStateDto(users, takeovers.Involving(sheetId, userId));
    }

    public async Task<SheetConnection?> LeaveAsync(string connectionId, CancellationToken cancellationToken)
    {
        var left = tracker.Leave(connectionId);
        if (left is not null)
        {
            await notifier.PresenceChangedAsync(left.SheetId, tracker.UsersOn(left.SheetId), cancellationToken);
        }

        return left;
    }
}
