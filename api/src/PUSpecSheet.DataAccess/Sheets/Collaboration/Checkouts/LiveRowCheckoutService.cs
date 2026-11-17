using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;
using PUSpecSheet.DataAccess.Sheets.Removal;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;

public sealed class LiveRowCheckoutService : ILiveRowCheckoutService
{
    private readonly IRowCheckouts _checkouts;
    private readonly SheetPresenceTracker _presence;
    private readonly LiveRowCheckoutTracker _tracker;
    private readonly IRowTakeoverSettler _takeovers;
    private readonly IRemovalInProgressGuard _removals;
    private readonly ISheetLiveNotifier _notifier;

    public LiveRowCheckoutService(
        IRowCheckouts checkouts,
        SheetPresenceTracker presence,
        LiveRowCheckoutTracker tracker,
        IRowTakeoverSettler takeovers,
        IRemovalInProgressGuard removals,
        ISheetLiveNotifier notifier)
    {
        _checkouts = checkouts;
        _presence = presence;
        _tracker = tracker;
        _takeovers = takeovers;
        _removals = removals;
        _notifier = notifier;
    }

    public async Task CheckOutAsync(string connectionId, int rowId, CancellationToken cancellationToken)
    {
        var connection = _presence.Find(connectionId)
            ?? throw new InvalidRequestException("Open a sheet before checking out a row.");

        var sheetId = await _checkouts.SheetOfAsync(rowId, cancellationToken)
            ?? throw new NotFoundException($"Row {rowId} was not found.");
        if (sheetId != connection.SheetId)
        {
            throw new InvalidRequestException("That row isn't on the sheet you have open.");
        }

        // A row someone else has changed is theirs already, whether or not they are still in it. Only who
        // holds the draft matters here, not whether it differs from what is published.
        if (await _checkouts.DraftHolderAsync(rowId, cancellationToken) is { } holderUserId && holderUserId != connection.UserId)
        {
            var names = await _checkouts.DisplayNamesAsync([holderUserId], cancellationToken);
            throw Held(names.GetValueOrDefault(holderUserId, "another user"));
        }

        await _removals.EnsureRowNotBeingRemovedAsync(rowId, connection.UserId, cancellationToken);

        var wanted = new LiveRowCheckout
        {
            SheetId = sheetId,
            RowId = rowId,
            UserId = connection.UserId,
            DisplayName = connection.DisplayName,
        };
        if (!_tracker.TryCheckOut(connectionId, wanted, out var heldBy))
        {
            throw Held(heldBy!.DisplayName);
        }

        await AnnounceAsync(sheetId, cancellationToken);
    }

    public async Task ReleaseAsync(string connectionId, CancellationToken cancellationToken)
    {
        var released = _tracker.Release(connectionId);
        if (released is not null)
        {
            await AnnounceAsync(released.SheetId, cancellationToken);
        }
    }

    // Tells the sheet who is in which row now. Entering a row leaves the one before, so either call may have freed a
    // row someone was waiting to take over; those requests have nothing left to wait for.
    private async Task AnnounceAsync(int sheetId, CancellationToken cancellationToken)
    {
        await _notifier.CheckoutsChangedAsync(sheetId, _tracker.OnSheet(sheetId), cancellationToken);
        await _takeovers.CloseBlockedAsync(sheetId, cancellationToken);
    }

    private static ConflictException Held(string holderName)
    {
        return new ConflictException($"This row is being edited by {holderName}.");
    }
}
