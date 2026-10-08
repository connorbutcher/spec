using PUSpecSheet.Contracts.Sheets.Collaboration;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public sealed class RowTakeoverSettler(
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

    public async Task CloseBlockedAsync(int sheetId, CancellationToken cancellationToken)
    {
        // Nearly always empty, and then this costs no query. When not, it is a request or two.
        foreach (var takeover in store.OnSheet(sheetId))
        {
            var obstacle = await closer.ObstacleAsync(takeover, cancellationToken);
            if (obstacle is not null && store.TryRemove(takeover))
            {
                await closer.CloseAsync(takeover, obstacle.Value, cancellationToken);
            }
        }
    }
}
