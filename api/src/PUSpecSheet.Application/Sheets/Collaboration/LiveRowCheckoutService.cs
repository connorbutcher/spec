using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class LiveRowCheckoutService(
    IRowCheckouts checkouts,
    SheetPresenceTracker presence,
    LiveRowCheckoutTracker tracker,
    IRowTakeoverSettler takeovers,
    ISheetLiveNotifier notifier) : ILiveRowCheckoutService
{
    public async Task CheckOutAsync(string connectionId, int rowId, CancellationToken cancellationToken)
    {
        var connection = presence.Find(connectionId)
            ?? throw new InvalidRequestException("Open a sheet before checking out a row.");

        var sheetId = await checkouts.SheetOfAsync(rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
        if (sheetId != connection.SheetId)
        {
            throw new InvalidRequestException("That row isn't on the sheet you have open.");
        }

        // A row someone else has changed is theirs already, whether or not they are still in it.
        var drafted = await checkouts.FindDraftAsync(rowId, cancellationToken);
        if (drafted is not null && drafted.HolderUserId != connection.UserId)
        {
            var names = await checkouts.DisplayNamesAsync([drafted.HolderUserId], cancellationToken);
            throw Held(names.GetValueOrDefault(drafted.HolderUserId, "another user"));
        }

        var wanted = new LiveRowCheckout(sheetId, rowId, connection.UserId, connection.DisplayName);
        if (!tracker.TryCheckOut(connectionId, wanted, out var heldBy))
        {
            throw Held(heldBy!.DisplayName);
        }

        await AnnounceAsync(sheetId, cancellationToken);
    }

    public async Task ReleaseAsync(string connectionId, CancellationToken cancellationToken)
    {
        var released = tracker.Release(connectionId);
        if (released is not null)
        {
            await AnnounceAsync(released.SheetId, cancellationToken);
        }
    }

    /// <summary>
    /// Tells the sheet who is in which row now. Entering a row leaves the one before, so either call may
    /// have freed a row someone was waiting to take over; those requests have nothing left to wait for.
    /// </summary>
    private async Task AnnounceAsync(int sheetId, CancellationToken cancellationToken)
    {
        await notifier.CheckoutsChangedAsync(sheetId, tracker.OnSheet(sheetId), cancellationToken);
        await takeovers.CloseBlockedAsync(sheetId, cancellationToken);
    }

    private static ConflictException Held(string holderName)
    {
        return new ConflictException($"This row is being edited by {holderName}.");
    }
}
