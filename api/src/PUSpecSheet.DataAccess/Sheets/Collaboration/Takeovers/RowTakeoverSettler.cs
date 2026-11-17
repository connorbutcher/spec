using PUSpecSheet.Abstractions.Sheets.Collaboration;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

public sealed class RowTakeoverSettler : IRowTakeoverSettler
{
    private readonly RowTakeoverStore _store;
    private readonly RowTakeoverCloser _closer;
    private readonly TimeProvider _clock;

    public RowTakeoverSettler(RowTakeoverStore store, RowTakeoverCloser closer, TimeProvider clock)
    {
        _store = store;
        _closer = closer;
        _clock = clock;
    }

    public async Task GrantOverdueAsync(CancellationToken cancellationToken)
    {
        foreach (var takeover in _store.Due(_clock.GetUtcNow().UtcDateTime))
        {
            if (_store.TryRemove(takeover))
            {
                await _closer.GrantAsync(takeover, RowTakeoverStatus.GrantedOnTimeout, cancellationToken);
            }
        }
    }

    public async Task CloseBlockedAsync(int sheetId, CancellationToken cancellationToken)
    {
        // Nearly always empty, and then this costs no query. When not, it is a request or two.
        foreach (var takeover in _store.OnSheet(sheetId))
        {
            var obstacle = await _closer.ObstacleAsync(takeover, cancellationToken);
            if (obstacle is not null && _store.TryRemove(takeover))
            {
                await _closer.CloseAsync(takeover, obstacle.Value, cancellationToken);
            }
        }
    }
}
