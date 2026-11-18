using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Legacy.SpecSheets;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Publishes the drafts of one submission as the sheet's next version: each becomes its item's published revision from
/// the version's moment, replacing the one before it. Other users' drafts are left as they are. The version gets a
/// published SpecSheets row with the sheet's property values, which the query API, the cache and copy-from know it by.
/// </summary>
internal class SheetDraftPublisher
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SheetDraftPublisher(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    /// <summary>
    /// Publishes the submission's drafts, in the caller's transaction, and saves.
    /// </summary>
    public async Task<DbSheetVersion> PublishAsync(DbSheetSubmission submission, int publishedByUserId, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submission, nameof(submission));

        var sheetId = submission.SheetId;
        var sheet = await _dbContext.Sheets
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == sheetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Sheet with Id {sheetId} not found.");

        // Whole seconds, so the moment is the same in every column that keeps it.
        var now = DateTime.UtcNow;
        now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond));

        var lastVersion = await _dbContext.SheetVersions
            .Where(v => v.SheetId == sheetId)
            .OrderByDescending(v => v.VersionNumber)
            .Select(v => new { v.VersionNumber, v.PublishedDateTime })
            .FirstOrDefaultAsync(cancellationToken);
        if (lastVersion is not null && lastVersion.PublishedDateTime >= now)
        {
            // Two versions in one second would share a moment; the later one is moved to the next second.
            now = lastVersion.PublishedDateTime.AddSeconds(1);
        }

        var header = new DbSpecSheet
        {
            Id = 0,
            SpecSheetPhaseId = sheet.SpecSheetPhaseId,
            SpecSheetTypeId = sheet.SpecSheetTypeId,
            IsPublished = true,
            CreatedByUserId = submission.SubmittedByUserId,
            CreatedDateTime = submission.SubmittedDateTime,
            HasBeenMigrated = false,
            PublishedByUserId = publishedByUserId,
            PublishedDateTime = now,
            ChangeLog = submission.Note
        };
        await _dbContext.SpecSheets.AddAsync(header, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var version = new DbSheetVersion
        {
            Id = 0,
            SheetId = sheetId,
            VersionNumber = (lastVersion?.VersionNumber ?? 0) + 1,
            PublishedDateTime = now,
            PublishedByUserId = publishedByUserId,
            ChangeLog = submission.Note,
            SourceSpecSheetId = header.Id,
            LegacySpecSheetId = null,
            HasBeenMigrated = false,
            SourceCreatedByUserId = submission.SubmittedByUserId,
            SourceCreatedDateTime = submission.SubmittedDateTime
        };
        await _dbContext.SheetVersions.AddAsync(version, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await PublishTablesAsync(sheetId, submission.Id, version, cancellationToken);
        await PublishGroupsAsync(sheetId, submission.Id, version, cancellationToken);
        await PublishRowsAsync(sheetId, submission.Id, version, cancellationToken);
        await PublishPropertiesAsync(sheetId, submission.Id, version, cancellationToken);
        await PublishLinkedTablesAsync(sheetId, submission.Id, version, cancellationToken);

        await CopyPropertyValuesToHeaderAsync(sheetId, header.Id, cancellationToken);
        return version;
    }

    // Each kind is published in two steps, the revision being replaced first, as an item has one current published revision.
    private async Task PublishTablesAsync(int sheetId, int submissionId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetTableRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId);
        await _dbContext.SheetTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetTableId == r.SheetTableId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishGroupsAsync(int sheetId, int submissionId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetTableGroupRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId);
        await _dbContext.SheetTableGroupRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetTableGroupId == r.SheetTableGroupId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishRowsAsync(int sheetId, int submissionId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetTableRowRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId);
        await _dbContext.SheetTableRowRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetTableRowId == r.SheetTableRowId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishPropertiesAsync(int sheetId, int submissionId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetPropertyRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId);
        await _dbContext.SheetPropertyRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetPropertyId == r.SheetPropertyId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishLinkedTablesAsync(int sheetId, int submissionId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetLinkedTableRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId);
        await _dbContext.SheetLinkedTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetLinkedTableId == r.SheetLinkedTableId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    // The lists of spec sheets read a version's property values from its SpecSheets row.
    private async Task CopyPropertyValuesToHeaderAsync(int sheetId, int specSheetId, CancellationToken cancellationToken)
    {
        var values = await _dbContext.SheetPropertyRevisions
            .AsNoTracking()
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null && !r.IsDeleted)
            .Join(_dbContext.SheetProperties, r => r.SheetPropertyId, p => p.Id, (r, p) => new { p.SpecSheetTypePropertyId, r.PropertyValue })
            .ToListAsync(cancellationToken);

        await _dbContext.SpecSheetPropertyValues.AddRangeAsync(values.Select(value => new DbSpecSheetPropertyValue
        {
            Id = 0,
            SpecSheetId = specSheetId,
            SpecSheetTypePropertyId = value.SpecSheetTypePropertyId,
            PropertyValue = value.PropertyValue
        }), cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
