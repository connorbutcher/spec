using Microsoft.Extensions.Logging;

namespace PUSpecSheet.Application.Sheets.Demo;

/// <summary>
/// Development-only sample data: a template and a sheet that uses it, so the whole flow can be tried as
/// soon as the API starts. Each part skips itself when its data already exists, and a failure is logged
/// instead of stopping the API from starting.
/// </summary>
public sealed partial class DemoDataSeeder(
    DemoLimitsTemplateSeeder templateSeeder,
    DemoLimitsSheetSeeder sheetSeeder,
    ILogger<DemoDataSeeder> logger)
{
    public async Task SeedAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await templateSeeder.SeedAsync(cancellationToken);
            await sheetSeeder.SeedAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            LogSeedFailed(logger, exception);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "The sample sheet data couldn't be added.")]
    private static partial void LogSeedFailed(ILogger logger, Exception exception);
}
