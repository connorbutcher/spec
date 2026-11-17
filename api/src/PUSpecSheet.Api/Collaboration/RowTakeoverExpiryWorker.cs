using PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>
/// Grants the takeover requests nobody answered in time, so a row can't stay stuck with someone who has
/// walked away from their screen.
/// </summary>
public sealed partial class RowTakeoverExpiryWorker : BackgroundService
{
    private static readonly TimeSpan _checkEvery = TimeSpan.FromSeconds(2);

    private readonly RowTakeoverStore _store;
    private readonly TimeProvider _clock;
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<RowTakeoverExpiryWorker> _logger;

    public RowTakeoverExpiryWorker(
        RowTakeoverStore store,
        TimeProvider clock,
        IServiceScopeFactory scopes,
        ILogger<RowTakeoverExpiryWorker> logger)
    {
        _store = store;
        _clock = clock;
        _scopes = scopes;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(_checkEvery, _clock);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (_store.Due(_clock.GetUtcNow().UtcDateTime).Count == 0)
            {
                continue;
            }

            try
            {
                await using var scope = _scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IRowTakeoverSettler>().GrantOverdueAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogGrantFailed(_logger, exception);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Couldn't grant the takeover requests that ran out of time.")]
    private static partial void LogGrantFailed(ILogger logger, Exception exception);
}
