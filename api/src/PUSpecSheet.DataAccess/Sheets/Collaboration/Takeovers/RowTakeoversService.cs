using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

public sealed class RowTakeoversService : IRowTakeoversService
{
    private const string NoLongerOpen = "That request is no longer open.";
    private const string UnknownUser = "Another user";

    private readonly IRowCheckouts _checkouts;
    private readonly ICurrentUser _currentUser;
    private readonly RowTakeoverStore _store;
    private readonly SheetPresenceTracker _presence;
    private readonly RowTakeoverCloser _closer;
    private readonly ISheetLiveNotifier _notifier;
    private readonly RowTakeoverOptions _options;
    private readonly TimeProvider _clock;

    public RowTakeoversService(
        IRowCheckouts checkouts,
        ICurrentUser currentUser,
        RowTakeoverStore store,
        SheetPresenceTracker presence,
        RowTakeoverCloser closer,
        ISheetLiveNotifier notifier,
        RowTakeoverOptions options,
        TimeProvider clock)
    {
        _checkouts = checkouts;
        _currentUser = currentUser;
        _store = store;
        _presence = presence;
        _closer = closer;
        _notifier = notifier;
        _options = options;
        _clock = clock;
    }

    public async Task<RowTakeoverAvailability> GetAvailabilityAsync(int rowId, CancellationToken cancellationToken)
    {
        // Asking the same question the request itself asks is what keeps the two from ever disagreeing.
        try
        {
            await CheckoutToAskForAsync(rowId, _currentUser.UserId, cancellationToken);
            return new RowTakeoverAvailability { IsAvailable = true };
        }
        catch (Exception refusal) when (refusal is ConflictException or InvalidRequestException)
        {
            return new RowTakeoverAvailability { IsAvailable = false, Reason = refusal.Message };
        }
    }

    public async Task<RowTakeover> RequestAsync(int rowId, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var checkout = await CheckoutToAskForAsync(rowId, me, cancellationToken);

        // Asking again for a row already asked for is the same request, not a second one.
        if (_store.ForRow(rowId) is { } mine)
        {
            return mine;
        }

        var takeover = await NewRequestAsync(rowId, checkout, me, cancellationToken);

        // Nobody is there to ask, and waiting a minute for silence helps no one.
        if (!_presence.IsPresent(takeover.SheetId, takeover.HolderUserId))
        {
            return await _closer.GrantAsync(takeover, RowTakeoverStatus.GrantedHolderAway, cancellationToken);
        }

        if (!_store.TryAdd(takeover))
        {
            throw new ConflictException("Someone else has just asked to take over this row.");
        }

        await _notifier.TakeoverChangedAsync(takeover, cancellationToken);
        return takeover;
    }

    public async Task<RowTakeover> ApproveAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = Take(takeoverId, RowTakeoverParty.Holder);
        return await _closer.GrantAsync(takeover, RowTakeoverStatus.Approved, cancellationToken);
    }

    public async Task<RowTakeover> DenyAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = Take(takeoverId, RowTakeoverParty.Holder);
        return await _closer.CloseAsync(takeover, RowTakeoverStatus.Denied, cancellationToken);
    }

    public async Task<RowTakeover> CancelAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = Take(takeoverId, RowTakeoverParty.Requester);
        return await _closer.CloseAsync(takeover, RowTakeoverStatus.Cancelled, cancellationToken);
    }

    // The row's checkout, once it is established that the user may ask for it. Every reason a request is refused is
    // here, so the availability check and the request can't disagree.
    private async Task<RowCheckoutState> CheckoutToAskForAsync(int rowId, int userId, CancellationToken cancellationToken)
    {
        var checkout = await _checkouts.FindAsync(rowId, cancellationToken);
        if (checkout is null)
        {
            if (await _checkouts.SheetOfAsync(rowId, cancellationToken) is null)
            {
                throw new NotFoundException($"Row {rowId} was not found.");
            }

            throw new ConflictException("This row is no longer checked out, so you can edit it straight away.");
        }

        if (checkout.HolderUserId == userId)
        {
            throw new InvalidRequestException("This row is already checked out to you.");
        }

        // A row someone has changed stays theirs until they publish: handing it on would hand on their work.
        if (checkout.HasChanges)
        {
            var names = await _checkouts.DisplayNamesAsync([checkout.HolderUserId], cancellationToken);
            var holder = names.GetValueOrDefault(checkout.HolderUserId, UnknownUser);
            throw new ConflictException($"{holder} has unpublished changes on this row. It can be taken over once they publish them or change them back.");
        }

        if (_store.ForRow(rowId) is { } waiting && waiting.RequesterUserId != userId)
        {
            throw new ConflictException($"{waiting.RequesterName} has already asked to take over this row.");
        }

        return checkout;
    }

    private async Task<RowTakeover> NewRequestAsync(int rowId, RowCheckoutState checkout, int requesterUserId, CancellationToken cancellationToken)
    {
        var names = await _checkouts.DisplayNamesAsync([requesterUserId, checkout.HolderUserId], cancellationToken);
        var now = _clock.GetUtcNow().UtcDateTime;
        return new RowTakeover
        {
            Id = Guid.NewGuid(),
            SheetId = checkout.SheetId,
            RowId = rowId,
            RequesterUserId = requesterUserId,
            RequesterName = names.GetValueOrDefault(requesterUserId, UnknownUser),
            HolderUserId = checkout.HolderUserId,
            HolderName = names.GetValueOrDefault(checkout.HolderUserId, UnknownUser),
            RequestedAtUtc = now,
            ExpiresAtUtc = now + _options.ResponseTime,
            Status = RowTakeoverStatus.Pending,
        };
    }

    // Takes a waiting request out of the store for the current user to settle, who must be the given party to it.
    // Taking it out first is what stops an answer and the timeout both settling it.
    private RowTakeover Take(Guid takeoverId, RowTakeoverParty party)
    {
        var takeover = _store.Find(takeoverId) ?? throw new NotFoundException(NoLongerOpen);

        var (partyUserId, refusal) = party == RowTakeoverParty.Holder
            ? (takeover.HolderUserId, "Only the person the row is checked out to can answer this request.")
            : (takeover.RequesterUserId, "Only the person who asked can withdraw this request.");
        if (partyUserId != _currentUser.UserId)
        {
            throw new InvalidRequestException(refusal);
        }

        return _store.TryRemove(takeover) ? takeover : throw new NotFoundException(NoLongerOpen);
    }
}
