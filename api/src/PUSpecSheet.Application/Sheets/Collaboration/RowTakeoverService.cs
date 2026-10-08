using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowTakeoverService(
    IRowCheckouts checkouts,
    ICurrentUser currentUser,
    RowTakeoverStore store,
    SheetPresenceTracker presence,
    RowTakeoverCloser closer,
    ISheetLiveNotifier notifier,
    RowTakeoverOptions options,
    TimeProvider clock) : IRowTakeoverService
{
    private const string NoLongerOpen = "That request is no longer open.";
    private const string UnknownUser = "Another user";

    public async Task<RowTakeoverAvailabilityDto> GetAvailabilityAsync(int rowId, CancellationToken cancellationToken)
    {
        // Asking the same question the request itself asks is what keeps the two from ever disagreeing.
        try
        {
            await CheckoutToAskForAsync(rowId, currentUser.UserId, cancellationToken);
            return new RowTakeoverAvailabilityDto(true, null);
        }
        catch (Exception refusal) when (refusal is ConflictException or InvalidRequestException)
        {
            return new RowTakeoverAvailabilityDto(false, refusal.Message);
        }
    }

    public async Task<RowTakeoverDto> RequestAsync(int rowId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var checkout = await CheckoutToAskForAsync(rowId, me, cancellationToken);

        // Asking again for a row already asked for is the same request, not a second one.
        if (store.ForRow(rowId) is { } mine)
        {
            return mine;
        }

        var takeover = await NewRequestAsync(rowId, checkout, me, cancellationToken);

        // Nobody is there to ask, and waiting a minute for silence helps no one.
        if (!presence.IsPresent(takeover.SheetId, takeover.HolderUserId))
        {
            return await closer.GrantAsync(takeover, RowTakeoverStatus.GrantedHolderAway, cancellationToken);
        }

        if (!store.TryAdd(takeover))
        {
            throw new ConflictException("Someone else has just asked to take over this row.");
        }

        await notifier.TakeoverChangedAsync(takeover, cancellationToken);
        return takeover;
    }

    public async Task<RowTakeoverDto> ApproveAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = Take(takeoverId, RowTakeoverParty.Holder);
        return await closer.GrantAsync(takeover, RowTakeoverStatus.Approved, cancellationToken);
    }

    public async Task<RowTakeoverDto> DenyAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = Take(takeoverId, RowTakeoverParty.Holder);
        return await closer.CloseAsync(takeover, RowTakeoverStatus.Denied, cancellationToken);
    }

    public async Task<RowTakeoverDto> CancelAsync(Guid takeoverId, CancellationToken cancellationToken)
    {
        var takeover = Take(takeoverId, RowTakeoverParty.Requester);
        return await closer.CloseAsync(takeover, RowTakeoverStatus.Cancelled, cancellationToken);
    }

    /// <summary>
    /// The row's checkout, once it is established that the user may ask for it. Every reason a request is
    /// refused is here, so the availability check and the request can't disagree.
    /// </summary>
    private async Task<RowCheckout> CheckoutToAskForAsync(int rowId, int userId, CancellationToken cancellationToken)
    {
        var checkout = await checkouts.FindAsync(rowId, cancellationToken);
        if (checkout is null)
        {
            if (!await checkouts.RowExistsAsync(rowId, cancellationToken))
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
            var names = await checkouts.DisplayNamesAsync([checkout.HolderUserId], cancellationToken);
            var holder = names.GetValueOrDefault(checkout.HolderUserId, UnknownUser);
            throw new ConflictException($"{holder} has unpublished changes on this row. It can be taken over once they publish or discard them.");
        }

        if (store.ForRow(rowId) is { } waiting && waiting.RequesterUserId != userId)
        {
            throw new ConflictException($"{waiting.RequesterName} has already asked to take over this row.");
        }

        return checkout;
    }

    private async Task<RowTakeoverDto> NewRequestAsync(int rowId, RowCheckout checkout, int requesterUserId, CancellationToken cancellationToken)
    {
        var names = await checkouts.DisplayNamesAsync([requesterUserId, checkout.HolderUserId], cancellationToken);
        var now = clock.GetUtcNow().UtcDateTime;
        return new RowTakeoverDto(
            Guid.NewGuid(),
            checkout.SheetId,
            rowId,
            requesterUserId,
            names.GetValueOrDefault(requesterUserId, UnknownUser),
            checkout.HolderUserId,
            names.GetValueOrDefault(checkout.HolderUserId, UnknownUser),
            now,
            now + options.ResponseTime,
            RowTakeoverStatus.Pending);
    }

    /// <summary>
    /// Takes a waiting request out of the store for the current user to settle, who must be the given
    /// party to it. Taking it out first is what stops an answer and the timeout both settling it.
    /// </summary>
    private RowTakeoverDto Take(Guid takeoverId, RowTakeoverParty party)
    {
        var takeover = store.Find(takeoverId) ?? throw new NotFoundException(NoLongerOpen);

        var (partyUserId, refusal) = party == RowTakeoverParty.Holder
            ? (takeover.HolderUserId, "Only the person the row is checked out to can answer this request.")
            : (takeover.RequesterUserId, "Only the person who asked can withdraw this request.");
        if (partyUserId != currentUser.UserId)
        {
            throw new InvalidRequestException(refusal);
        }

        return store.TryRemove(takeover) ? takeover : throw new NotFoundException(NoLongerOpen);
    }
}
