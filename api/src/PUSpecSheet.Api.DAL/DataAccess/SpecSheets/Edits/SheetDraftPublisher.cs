using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Spec;
using PUSpecSheet.Api.Database.Schema.SpecSheets;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Publishes a sheet's drafts as its next version: each draft becomes the item's published revision from the version's
/// moment, replacing the one before it. The draft's SpecSheets row becomes the version's, as a published copy was.
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
    /// Publishes the drafts and the header, in the caller's transaction, and saves.
    /// </summary>
    public async Task<DbSheetVersion> PublishAsync(int sheetId, DbSpecSheet header, int publishedByUserId, string? changeLog, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(header, nameof(header));

        // Whole seconds, so the moment is the same in every column that keeps it.
        var now = DateTime.UtcNow;
        now = now.AddTicks(-(now.Ticks % TimeSpan.TicksPerSecond));

        var lastVersionNumber = await _dbContext.SheetVersions
            .Where(v => v.SheetId == sheetId)
            .MaxAsync(v => (int?)v.VersionNumber, cancellationToken);

        var version = new DbSheetVersion
        {
            Id = 0,
            SheetId = sheetId,
            VersionNumber = (lastVersionNumber ?? 0) + 1,
            PublishedDateTime = now,
            PublishedByUserId = publishedByUserId,
            ChangeLog = changeLog,
            SourceSpecSheetId = header.Id,
            LegacySpecSheetId = header.LegacySpecSheetId,
            HasBeenMigrated = header.HasBeenMigrated,
            SourceCreatedByUserId = header.CreatedByUserId,
            SourceCreatedDateTime = header.CreatedDateTime
        };

        header.IsPublished = true;
        header.PublishedByUserId = publishedByUserId;
        header.PublishedDateTime = now;
        header.ChangeLog = changeLog;

        await _dbContext.SheetVersions.AddAsync(version, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await PublishTablesAsync(sheetId, version, cancellationToken);
        await PublishGroupsAsync(sheetId, version, cancellationToken);
        await PublishRowsAsync(sheetId, version, cancellationToken);
        await PublishPropertiesAsync(sheetId, version, cancellationToken);
        await PublishLinkedTablesAsync(sheetId, version, cancellationToken);

        return version;
    }

    // Each kind is published in two steps, the revision being replaced first, as an item has one current published revision.
    private async Task PublishTablesAsync(int sheetId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetTableRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft);
        await _dbContext.SheetTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetTableId == r.SheetTableId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishGroupsAsync(int sheetId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetTableGroupRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft);
        await _dbContext.SheetTableGroupRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetTableGroupId == r.SheetTableGroupId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishRowsAsync(int sheetId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetTableRowRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft);
        await _dbContext.SheetTableRowRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetTableRowId == r.SheetTableRowId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishPropertiesAsync(int sheetId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetPropertyRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft);
        await _dbContext.SheetPropertyRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetPropertyId == r.SheetPropertyId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }

    private async Task PublishLinkedTablesAsync(int sheetId, DbSheetVersion version, CancellationToken cancellationToken)
    {
        var drafts = _dbContext.SheetLinkedTableRevisions.Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft);
        await _dbContext.SheetLinkedTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Published && r.SupersededDateTime == null
                && drafts.Any(d => d.SheetLinkedTableId == r.SheetLinkedTableId))
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SupersededDateTime, version.PublishedDateTime), cancellationToken);
        await drafts.ExecuteUpdateAsync(setters => setters
            .SetProperty(r => r.Status, RevisionStatus.Published)
            .SetProperty(r => r.PublishedDateTime, version.PublishedDateTime)
            .SetProperty(r => r.SheetVersionId, version.Id), cancellationToken);
    }
}