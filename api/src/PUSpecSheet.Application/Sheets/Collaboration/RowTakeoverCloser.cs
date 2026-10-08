using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

/// <summary>
/// Brings a takeover request to its end and tells the two people involved. Every way a request can end
/// (an answer, a withdrawal, the timeout, the row being released) goes through here, so each ends the
/// same way. The caller has already taken the request out of <see cref="RowTakeoverStore"/>.
/// </summary>
public sealed class RowTakeoverCloser(IRowCheckouts checkouts, ISheetLiveNotifier notifier)
{
    /// <summary>
    /// Hands the row to the requester and closes the request with <paramref name="status"/>. If the holder
    /// no longer holds the row there is nothing to hand over, and it closes as released instead.
    /// </summary>
    public async Task<RowTakeoverDto> GrantAsync(RowTakeoverDto takeover, RowTakeoverStatus status, CancellationToken cancellationToken)
    {
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
}
