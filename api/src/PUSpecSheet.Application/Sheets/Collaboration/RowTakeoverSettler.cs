using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowTakeoverSettler(
    IRowCheckouts checkouts,
    RowTakeoverStore store,
    RowTakeoverCloser closer,
    TimeProvider clock) : IRowTakeoverSettler
{
    public async Task GrantOverdueAsync(CancellationToken cancellationToken)
    {
        foreach (var takeover in store.Due(clock.GetUtcNow().UtcDateTime))
        {
            if (store.TryRemove(takeover))
            {
                await closer.GrantAsync(takeover, RowTakeoverStatus.GrantedOnTimeout, cancellationToken);
            }
        }
    }

    public async Task ReleaseSettledAsync(int sheetId, CancellationToken cancellationToken)
    {
        // Nearly always empty, and then this costs no query.
        var waiting = store.OnSheet(sheetId);
        if (waiting.Count == 0)
        {
            return;
        }

        var holders = await checkouts.HoldersAsync(waiting.Select(takeover => takeover.RowId).ToList(), cancellationToken);
        foreach (var takeover in waiting)
        {
            var stillHeld = holders.TryGetValue(takeover.RowId, out var holder) && holder == takeover.HolderUserId;
            if (!stillHeld && store.TryRemove(takeover))
            {
                await closer.CloseAsync(takeover, RowTakeoverStatus.Released, cancellationToken);
            }
        }
    }
}
