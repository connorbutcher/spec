using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Migrator.Building;
using PUSpecSheet.Migrator.Legacy;
using PUSpecSheet.Migrator.Verification;

namespace PUSpecSheet.Migrator;

/// <summary>
/// Brings one sheet's revisions into step with its copies, in its own transaction. A sheet whose copies have changed
/// since it was last migrated (or that hasn't been migrated) is built again from them: what was built before is
/// deleted, the history is built and saved, read back as of every version (and with its drafts) and compared with each
/// copy. It commits only when every check matches and --commit was given.
/// </summary>
internal sealed class SheetMigrator
{
    private readonly IDbContextFactory<PUSpecSheetDbContext> _dbFactory;
    private readonly LegacySheetReader _legacy;
    private readonly MigratorOptions _options;
    private readonly ILogger _logger;

    public SheetMigrator(IDbContextFactory<PUSpecSheetDbContext> dbFactory, LegacySheetReader legacy, MigratorOptions options, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(dbFactory, nameof(dbFactory));
        ArgumentNullException.ThrowIfNull(legacy, nameof(legacy));
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _dbFactory = dbFactory;
        _legacy = legacy;
        _options = options;
        _logger = logger;
    }

    public async Task<SheetOutcome> MigrateAsync(LegacySheet legacySheet, CancellationToken cancellationToken)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        var existing = await db.Sheets.AsNoTracking()
            .Where(sheet => sheet.SpecSheetPhaseId == legacySheet.PhaseId && sheet.SpecSheetTypeId == legacySheet.SheetTypeId)
            .Select(sheet => new { sheet.Id, sheet.ReferenceId, sheet.LegacySourceHash })
            .SingleOrDefaultAsync(cancellationToken);

        // A sheet without a hash was made by the app, not built from its copies, so it isn't the migrator's to replace.
        if (existing is { LegacySourceHash: null })
        {
            _logger.NotMigrated(legacySheet.Name);
            return SheetOutcome.NotMigrated;
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

        var hash = LegacyFingerprint.Of(legacy);
        if (!_options.Rebuild && existing?.LegacySourceHash is { } built && built.AsSpan().SequenceEqual(hash))
        {
            _logger.UpToDate(legacySheet.Name);
            return SheetOutcome.UpToDate;
        }

        // Disposing the transaction without committing it rolls everything back, the deletion included.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        BuiltSheet history;
        try
        {
            if (existing is not null)
            {
                await DeleteAsync(db, existing.Id, cancellationToken);
            }

            var now = DateTime.UtcNow;
            var sheet = new DbSheet
            {
                Id = 0,
                SpecSheetPhaseId = legacySheet.PhaseId,
                SpecSheetTypeId = legacySheet.SheetTypeId,
                ReferenceId = existing?.ReferenceId ?? Guid.NewGuid(),
                CreatedDateTime = legacySheet.CreatedDateTime,
                LegacySourceHash = hash,
                MigratedDateTime = now
            };
            db.Sheets.Add(sheet);
            await db.SaveChangesAsync(cancellationToken);

            history = SheetHistoryBuilder.Build(sheet, legacy.Copies, now);
            await db.SaveChangesAsync(cancellationToken);
            await SaveSubmissionsAsync(db, history, cancellationToken);
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

        foreach (var warning in legacy.Warnings.Concat(history.Warnings))
        {
            _logger.SheetWarning(legacySheet.Name, warning);
        }

        var revisions = CountRevisions(history.Sheet);
        var sheetId = history.Sheet.Id;
        db.ChangeTracker.Clear();

        var matches = true;
        foreach (var check in history.Checks)
        {
            var actual = await TargetSnapshotReader.ReadAsync(db, sheetId, check.Moment, cancellationToken);
            if (SheetVerifier.Difference(check.Expected, actual) is { } difference)
            {
                var which = check.Moment is null ? "the working copy (drafts)" : $"version {check.VersionNumber}";
                _logger.Mismatch(legacySheet.Name, which, difference);
                matches = false;
            }
        }

        var outcome = !matches ? SheetOutcome.Failed : _options.Commit ? SheetOutcome.Migrated : SheetOutcome.Checked;
        if (outcome == SheetOutcome.Migrated)
        {
            await transaction.CommitAsync(cancellationToken);
        }

        _logger.Done(legacySheet.Name, legacy.Copies.Count, history.Sheet.Versions.Count, revisions, history.Submissions.Count, outcome);
        return outcome;
    }

    // Deletes what was built for a sheet before, values first and the sheet last, whose own deletion takes its items
    // and versions with it.
    private static async Task DeleteAsync(PUSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        var rowRevisions = db.SheetTableRowRevisions.Where(revision => revision.SheetId == sheetId);
        await db.SheetTextValues.Where(value => rowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId)).ExecuteDeleteAsync(cancellationToken);
        await db.SheetNumericValues.Where(value => rowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId)).ExecuteDeleteAsync(cancellationToken);
        await db.SheetPairValues.Where(value => rowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId)).ExecuteDeleteAsync(cancellationToken);
        await db.SheetLinkedSheetValues.Where(value => rowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId)).ExecuteDeleteAsync(cancellationToken);
        await db.SheetCellConfigurationOverrides.Where(value => rowRevisions.Any(revision => revision.Id == value.SheetTableRowRevisionId)).ExecuteDeleteAsync(cancellationToken);

        await db.SheetTableRowRevisions.Where(revision => revision.SheetId == sheetId).ExecuteDeleteAsync(cancellationToken);
        await db.SheetTableGroupRevisions.Where(revision => revision.SheetId == sheetId).ExecuteDeleteAsync(cancellationToken);
        await db.SheetTableRevisions.Where(revision => revision.SheetId == sheetId).ExecuteDeleteAsync(cancellationToken);
        await db.SheetPropertyRevisions.Where(revision => revision.SheetId == sheetId).ExecuteDeleteAsync(cancellationToken);
        await db.SheetLinkedTableRevisions.Where(revision => revision.SheetId == sheetId).ExecuteDeleteAsync(cancellationToken);
        await db.SheetExternalReferenceRevisions.Where(revision => revision.SheetId == sheetId).ExecuteDeleteAsync(cancellationToken);

        await db.SheetSubmissions.Where(submission => submission.SheetId == sheetId).ExecuteDeleteAsync(cancellationToken);
        await db.Sheets.Where(sheet => sheet.Id == sheetId).ExecuteDeleteAsync(cancellationToken);
    }

    // Submissions go in once the versions they published have ids, and then their revisions are pointed at them.
    private static async Task SaveSubmissionsAsync(PUSpecSheetDbContext db, BuiltSheet history, CancellationToken cancellationToken)
    {
        if (history.Submissions.Count == 0)
        {
            return;
        }

        foreach (var planned in history.Submissions)
        {
            planned.Submission.SheetVersionId = planned.Version?.Id;
            db.SheetSubmissions.Add(planned.Submission);
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var planned in history.Submissions)
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
                + table.Groups.Sum(group => group.Revisions.Count + group.Rows.Sum(row => row.Revisions.Count)))
            + sheet.Properties.Sum(property => property.Revisions.Count)
            + sheet.LinkedTables.Sum(link => link.Revisions.Count)
            + sheet.ExternalReferences.Sum(reference => reference.Revisions.Count);
    }
}
