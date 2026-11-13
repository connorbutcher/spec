using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Database;
using PUSpecSheet.Migrator;
using PUSpecSheet.Migrator.Legacy;

using var loggerFactory = LoggerFactory.Create(logging => logging.AddSimpleConsole(console => console.SingleLine = true));
var logger = loggerFactory.CreateLogger("PUSpecSheet.Migrator");

var options = MigratorOptions.Parse(args);
if (options is null)
{
    logger.BadArguments(MigratorOptions.Usage);
    return 2;
}

using var cancellation = new CancellationTokenSource();
Console.CancelKeyPress += (_, eventArgs) =>
{
    // Abandon the sheet in hand, whose transaction then rolls back, rather than killing the process mid-save.
    eventArgs.Cancel = true;
    cancellation.Cancel();
};

await using var legacyConnection = new SqlConnection(options.LegacyConnectionString);
await legacyConnection.OpenAsync(cancellation.Token);
var reader = new LegacySheetReader(legacyConnection);

var targetOptions = new DbContextOptionsBuilder<PuSpecSheetDbContext>()
    .UseSqlServer(options.TargetConnectionString)
    .ConfigureWarnings(warnings => warnings.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS))
    .Options;
var migrator = new SheetMigrator(() => new PuSpecSheetDbContext(targetOptions), reader, options.Commit, logger);

var sheets = (await reader.ListSheetsAsync(cancellation.Token))
    .Where(sheet => options.SheetIds.Count == 0 || options.SheetIds.Contains(sheet.Id))
    .ToList();
logger.Starting(sheets.Count, options.Commit ? "keeping what is migrated" : "dry run, nothing is kept");

var outcomes = new List<SheetOutcome>();
foreach (var sheet in sheets)
{
    outcomes.Add(await migrator.MigrateAsync(sheet, cancellation.Token));
}

logger.Finished(
    outcomes.Count(outcome => outcome == SheetOutcome.Migrated),
    outcomes.Count(outcome => outcome == SheetOutcome.Checked),
    outcomes.Count(outcome => outcome == SheetOutcome.Skipped),
    outcomes.Count(outcome => outcome == SheetOutcome.Failed));

return outcomes.Contains(SheetOutcome.Failed) ? 1 : 0;
