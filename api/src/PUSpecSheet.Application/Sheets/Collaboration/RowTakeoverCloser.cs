using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Brings a takeover request to its end and tells the two people involved. Every way a request can end
/// (an answer, a withdrawal, the timeout, the row being released or changed) goes through here, so each
/// ends the same way. The caller has already taken the request out of <see cref="RowTakeoverStore"/>.
/// </summary>
public sealed class RowTakeoverCloser(IRowCheckouts checkouts, ISheetLiveNotifier notifier)
{
    /// <summary>
    /// Hands the row to the requester and closes the request with <paramref name="status"/>, if the row
    /// can still be handed over. It can't when the holder no longer holds it (closed as released) or has
    /// changed it since the request was made (closed as kept: a changed row stays with whoever changed it
    /// until they publish).
    /// </summary>
    public async Task<RowTakeoverDto> GrantAsync(RowTakeoverDto takeover, RowTakeoverStatus status, CancellationToken cancellationToken)
    {
        var obstacle = await ObstacleAsync(takeover, cancellationToken);
        if (obstacle is not null)
        {
            return await CloseAsync(takeover, obstacle.Value, cancellationToken);
        }

        var handedOver = await checkouts.TransferAsync(
            takeover.RowId,
            takeover.HolderUserId,
            takeover.RequesterUserId,
            cancellationToken);
        if (!handedOver)
        {
            return await CloseAsync(takeover, RowTakeoverStatus.Released, cancellationToken);
        }

        var granted = await CloseAsync(takeover, status, cancellationToken);

        // Everyone on the sheet, not just these two, now sees the row checked out to someone else.
        await notifier.SheetChangedAsync(takeover.SheetId, exceptConnectionId: null, cancellationToken);
        return granted;
    }

    /// <summary>Closes the request with <paramref name="status"/>, leaving the row where it is.</summary>
    public async Task<RowTakeoverDto> CloseAsync(RowTakeoverDto takeover, RowTakeoverStatus status, CancellationToken cancellationToken)
    {
        var closed = takeover with { Status = status };
        await notifier.TakeoverChangedAsync(closed, cancellationToken);
        return closed;
    }

    /// <summary>
    /// Why the row can no longer be handed over, as the status the request closes with, or null when it
    /// still can be. Also asked while a request waits, to close it as soon as it can't succeed.
    /// </summary>
    public async Task<RowTakeoverStatus?> ObstacleAsync(RowTakeoverDto takeover, CancellationToken cancellationToken)
    {
        var checkout = await checkouts.FindAsync(takeover.RowId, cancellationToken);
        if (checkout is null || checkout.HolderUserId != takeover.HolderUserId)
        {
            return RowTakeoverStatus.Released;
        }

        return checkout.HasChanges ? RowTakeoverStatus.KeptForChanges : null;
    }
}
