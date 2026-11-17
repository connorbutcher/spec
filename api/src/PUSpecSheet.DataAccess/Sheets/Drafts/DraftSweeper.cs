using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

// Drops the drafts on a sheet that make no difference to what is published, such as a row locked or edited and then
// put back. Normally a draft is released the moment it is put back; this catches ones left from before that, so they
// don't light up Publish or hold a lock for nothing. It takes the current user's drafts, or everyone's ahead of a
// publish of everything.
public sealed class DraftSweeper
{
    private readonly ICurrentUser _currentUser;
    private readonly PuSpecSheetDbContext _db;
    private readonly TableDrafts _tables;
    private readonly SectionDrafts _sections;
    private readonly RowDrafts _rows;
    private readonly ColumnBlockDrafts _columnBlocks;

    public DraftSweeper(
        PuSpecSheetDbContext db,
        ICurrentUser currentUser,
        TableDrafts tables,
        SectionDrafts sections,
        RowDrafts rows,
        ColumnBlockDrafts columnBlocks)
    {
        _currentUser = currentUser;
        _db = db;
        _tables = tables;
        _sections = sections;
        _rows = rows;
        _columnBlocks = columnBlocks;
    }

    public Task SweepAsync(int sheetId, CancellationToken cancellationToken)
    {
        return SweepAsync(sheetId, PublishScope.Mine, cancellationToken);
    }

    public async Task SweepAsync(int sheetId, PublishScope scope, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var everyone = scope == PublishScope.All;

        var rowIds = await _db.SheetRowRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetRowId, cancellationToken);
        await _rows.ReleaseUnchangedAsync(rowIds, everyone, cancellationToken);

        var sectionIds = await _db.SheetSectionRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetSectionId, cancellationToken);
        await _sections.ReleaseUnchangedAsync(sectionIds, everyone, cancellationToken);

        var tableIds = await _db.SheetTableRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetTableId, cancellationToken);
        await _tables.ReleaseUnchangedAsync(tableIds, everyone, cancellationToken);

        var columnBlockIds = await _db.SheetColumnBlockRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetColumnBlockId, cancellationToken);
        await _columnBlocks.ReleaseUnchangedAsync(columnBlockIds, everyone, cancellationToken);
    }
}
