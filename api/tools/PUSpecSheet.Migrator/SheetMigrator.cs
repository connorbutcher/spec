using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Migrator.Building;
using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Verification;

namespace PUSpecSheet.Migrator;

/// <summary>
/// Migrates one legacy sheet at a time, each in its own transaction: reads every copy, builds the revisions
/// (<see cref="SheetHistoryBuilder"/>), saves them, then reads the sheet back as of every version, and with its
/// drafts, and compares each with the copy it came from. The transaction is committed only when every one
/// matches and this isn't a dry run, so a sheet is either migrated whole and checked, or not at all. A sheet
/// whose public id is already in the target database is skipped, so the migrator can be run again after a
/// failure and carries on where it stopped.
/// </summary>
internal sealed class SheetMigrator
{
    private readonly Func<PuSpecSheetDbContext> createTarget;
    private readonly LegacySheetReader legacy;
    private readonly bool commit;
    private readonly ILogger logger;

    public SheetMigrator(Func<PuSpecSheetDbContext> createTarget, LegacySheetReader legacy, bool commit, ILogger logger)
    {
        this.createTarget = createTarget;
        this.legacy = legacy;
        this.commit = commit;
        this.logger = logger;
    }

    public async Task<SheetOutcome> MigrateAsync(LegacySheet legacySheet, CancellationToken cancellationToken)
    {
        await using var db = createTarget();
        if (await db.Sheets.AnyAsync(sheet => sheet.PublicId == legacySheet.PublicId, cancellationToken))
        {
            logger.Skipped(legacySheet.Id, legacySheet.PublicId);
            return SheetOutcome.Skipped;
        }

        var copies = await legacy.LoadCopiesAsync(legacySheet.Id, cancellationToken);

        // Disposing the transaction without committing it rolls everything back.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        BuiltSheet built;
        try
        {
            var sheet = new DbSheet
            {
                PublicId = legacySheet.PublicId,
                PhaseId = legacySheet.PhaseId,
                SheetTypeId = legacySheet.SheetTypeId,
                CreatedAtUtc = legacySheet.CreatedAtUtc,
            };
            db.Sheets.Add(sheet);
            await db.SaveChangesAsync(cancellationToken);

            built = SheetHistoryBuilder.Build(sheet, copies, DateTime.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (MigrationException exception)
        {
            logger.CantMigrate(legacySheet.Id, exception.Message);
            return SheetOutcome.Failed;
        }
        catch (DbUpdateException exception)
        {
            logger.SaveFailed(legacySheet.Id, exception);
            return SheetOutcome.Failed;
        }

        foreach (var warning in built.Warnings)
        {
            logger.SheetWarning(legacySheet.Id, warning);
        }

        var revisions = CountRevisions(built.Sheet);
        var sheetId = built.Sheet.Id;
        db.ChangeTracker.Clear();

        var matches = true;
        foreach (var check in built.Checks)
        {
            var actual = await TargetSnapshotReader.ReadAsync(db, sheetId, check.MomentUtc, cancellationToken);
            if (SheetVerifier.Difference(check.Expected, actual) is { } difference)
            {
                var which = check.MomentUtc is null ? "the working copy (drafts)" : $"version {check.VersionNumber}";
                logger.Mismatch(legacySheet.Id, which, difference);
                matches = false;
            }
        }

        var outcome = !matches ? SheetOutcome.Failed : commit ? SheetOutcome.Migrated : SheetOutcome.Checked;
        if (outcome == SheetOutcome.Migrated)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        logger.Done(legacySheet.Id, copies.Count, built.Sheet.Versions.Count, revisions, outcome);
        return outcome;
    }

    private static int CountRevisions(DbSheet sheet)
    {
        return sheet.Tables.Sum(table => table.Revisions.Count
            + table.ColumnBlocks.Sum(block => block.Revisions.Count)
            + table.Sections.Sum(section => section.Revisions.Count + section.Rows.Sum(row => row.Revisions.Count)));
    }
}
