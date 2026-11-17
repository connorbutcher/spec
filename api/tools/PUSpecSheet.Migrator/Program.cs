using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Api.Database;
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

var connectionString = options.ConnectionString;
var services = new ServiceCollection();
services.AddDbContextFactory<PUSpecSheetDbContext>(db => db
    .UseSqlServer(connectionString)
    .ConfigureWarnings(warnings => warnings.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS)));
await using var provider = services.BuildServiceProvider();
var dbFactory = provider.GetRequiredService<IDbContextFactory<PUSpecSheetDbContext>>();

await using var legacyDb = await dbFactory.CreateDbContextAsync(cancellation.Token);
var reader = new LegacySheetReader(legacyDb);
var migrator = new SheetMigrator(dbFactory, reader, options, logger);

var sheets = (await reader.ListSheetsAsync(cancellation.Token))
    .Where(sheet => options.PhaseIds.Count == 0 || options.PhaseIds.Contains(sheet.PhaseId))
    .ToList();
logger.Starting(sheets.Count, options.Commit ? "keeping what is built" : "dry run, nothing is kept");

var outcomes = new List<SheetOutcome>();
foreach (var sheet in sheets)
{
    outcomes.Add(await migrator.MigrateAsync(sheet, cancellation.Token));
}

logger.Finished(
    outcomes.Count(outcome => outcome == SheetOutcome.Migrated),
    outcomes.Count(outcome => outcome == SheetOutcome.Checked),
    outcomes.Count(outcome => outcome == SheetOutcome.UpToDate),
    outcomes.Count(outcome => outcome == SheetOutcome.NotMigrated),
    outcomes.Count(outcome => outcome == SheetOutcome.Failed));

return outcomes.Contains(SheetOutcome.Failed) ? 1 : 0;
