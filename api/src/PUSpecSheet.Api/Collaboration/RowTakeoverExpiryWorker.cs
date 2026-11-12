using PUSpecSheet.Application.Sheets.Collaboration;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>
/// Grants the takeover requests nobody answered in time, so a row can't stay stuck with someone who has
/// walked away from their screen.
/// </summary>
public sealed partial class RowTakeoverExpiryWorker : BackgroundService
{
    private static readonly TimeSpan CheckEvery = TimeSpan.FromSeconds(2);

    private readonly RowTakeoverStore store;
    private readonly TimeProvider clock;
    private readonly IServiceScopeFactory scopes;
    private readonly ILogger<RowTakeoverExpiryWorker> logger;

    public RowTakeoverExpiryWorker(
        RowTakeoverStore store,
        TimeProvider clock,
        IServiceScopeFactory scopes,
        ILogger<RowTakeoverExpiryWorker> logger)
    {
        this.store = store;
        this.clock = clock;
        this.scopes = scopes;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckEvery, clock);
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            if (store.Due(clock.GetUtcNow().UtcDateTime).Count == 0)
            {
                continue;
            }

            try
            {
                await using var scope = scopes.CreateAsyncScope();
                await scope.ServiceProvider.GetRequiredService<IRowTakeoverSettler>().GrantOverdueAsync(stoppingToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                LogGrantFailed(logger, exception);
            }
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Couldn't grant the takeover requests that ran out of time.")]
    private static partial void LogGrantFailed(ILogger logger, Exception exception);
}
