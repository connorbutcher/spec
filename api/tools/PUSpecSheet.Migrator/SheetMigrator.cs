using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;
using PUSpecSheet.Migrator.Building;
using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Verification;

namespace PUSpecSheet.Migrator;

// Migrates one legacy sheet in its own transaction: reads every copy, builds the history, saves it, reads it back as
// of every version (and with its drafts) and compares each with its legacy copy. It commits only when every check
// matches and --commit was given.
internal sealed class SheetMigrator
{
    private readonly Func<PuSpecSheetDbContext> _createTarget;
    private readonly LegacySheetReader _legacy;
    private readonly bool _commit;
    private readonly ILogger _logger;

    public SheetMigrator(Func<PuSpecSheetDbContext> createTarget, LegacySheetReader legacy, bool commit, ILogger logger)
    {
        _createTarget = createTarget;
        _legacy = legacy;
        _commit = commit;
        _logger = logger;
    }

    public async Task<SheetOutcome> MigrateAsync(LegacySheet legacySheet, CancellationToken cancellationToken)
    {
        await using var db = _createTarget();

        // Legacy sheets have no public id; a sheet of the same phase and type already there is taken to be this one.
        if (await db.Sheets.AnyAsync(sheet => sheet.PhaseId == legacySheet.PhaseId && sheet.SheetTypeId == legacySheet.SheetTypeId, cancellationToken))
        {
            _logger.Skipped(legacySheet.Name);
            return SheetOutcome.Skipped;
        }

        LegacyCopies legacy;
        try
        {
            legacy = await _legacy.LoadCopiesAsync(legacySheet, cancellationToken);
        }
        catch (MigrationException exception)
        {
            _logger.CantMigrate(legacySheet.Name, exception.Message);
            return SheetOutcome.Failed;
        }

        // Disposing the transaction without committing it rolls everything back.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        BuiltSheet built;
        try
        {
            var sheet = new DbSheet
            {
                PhaseId = legacySheet.PhaseId,
                SheetTypeId = legacySheet.SheetTypeId,
                CreatedAtUtc = legacySheet.CreatedAtUtc,
            };
            db.Sheets.Add(sheet);
            await db.SaveChangesAsync(cancellationToken);

            built = SheetHistoryBuilder.Build(sheet, legacy.Copies, DateTime.UtcNow);
            await db.SaveChangesAsync(cancellationToken);
            await SaveSubmissionsAsync(db, built, cancellationToken);
        }
        catch (MigrationException exception)
        {
            _logger.CantMigrate(legacySheet.Name, exception.Message);
            return SheetOutcome.Failed;
        }
        catch (DbUpdateException exception)
        {
            _logger.SaveFailed(legacySheet.Name, exception);
            return SheetOutcome.Failed;
        }

        foreach (var warning in legacy.Warnings.Concat(built.Warnings))
        {
            _logger.SheetWarning(legacySheet.Name, warning);
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
                _logger.Mismatch(legacySheet.Name, which, difference);
                matches = false;
            }
        }

        var outcome = !matches ? SheetOutcome.Failed : _commit ? SheetOutcome.Migrated : SheetOutcome.Checked;
        if (outcome == SheetOutcome.Migrated)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        _logger.Done(legacySheet.Name, legacy.Copies.Count, built.Sheet.Versions.Count, revisions, built.Submissions.Count, outcome);
        return outcome;
    }

    // Submissions go in once the versions they published have ids, and then their revisions are pointed at them.
    private static async Task SaveSubmissionsAsync(PuSpecSheetDbContext db, BuiltSheet built, CancellationToken cancellationToken)
    {
        if (built.Submissions.Count == 0)
        {
            return;
        }

        foreach (var planned in built.Submissions)
        {
            planned.Submission.SheetVersionId = planned.Version?.Id;
            db.SheetSubmissions.Add(planned.Submission);
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var planned in built.Submissions)
        {
            foreach (var revision in planned.Revisions)
            {
                revision.SheetSubmissionId = planned.Submission.Id;
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static int CountRevisions(DbSheet sheet)
    {
        return sheet.Tables.Sum(table => table.Revisions.Count
                + table.ColumnBlocks.Sum(block => block.Revisions.Count)
                + table.Sections.Sum(section => section.Revisions.Count + section.Rows.Sum(row => row.Revisions.Count)))
            + sheet.Properties.Sum(property => property.Revisions.Count)
            + sheet.LinkedTables.Sum(linked => linked.Revisions.Count)
            + sheet.ExternalReferences.Sum(reference => reference.Revisions.Count);
    }
}
