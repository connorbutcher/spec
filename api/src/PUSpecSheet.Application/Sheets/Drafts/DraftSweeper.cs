using PUSpecSheet.Application.Sheets.Queries;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>
/// Drops the drafts on a sheet that make no difference to what is published, such as a row locked or edited
/// and then put back. Normally a draft is released the moment it is put back; this catches ones left from
/// before that, so they don't light up Publish or hold a lock for nothing. It takes the current user's
/// drafts, or everyone's ahead of a publish of everything.
/// </summary>
public sealed class DraftSweeper
{
    private readonly ICurrentUser currentUser;
    private readonly PuSpecSheetDbContext db;
    private readonly TableDrafts tables;
    private readonly SectionDrafts sections;
    private readonly RowDrafts rows;
    private readonly ColumnBlockDrafts columnBlocks;

    public DraftSweeper(
        PuSpecSheetDbContext db,
        ICurrentUser currentUser,
        TableDrafts tables,
        SectionDrafts sections,
        RowDrafts rows,
        ColumnBlockDrafts columnBlocks)
    {
        this.currentUser = currentUser;
        this.db = db;
        this.tables = tables;
        this.sections = sections;
        this.rows = rows;
        this.columnBlocks = columnBlocks;
    }

    public Task SweepAsync(int sheetId, CancellationToken cancellationToken)
    {
        return SweepAsync(sheetId, PublishScope.Mine, cancellationToken);
    }

    public async Task SweepAsync(int sheetId, PublishScope scope, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var everyone = scope == PublishScope.All;

        var rowIds = await db.SheetRowRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetRowId, cancellationToken);
        await rows.ReleaseUnchangedAsync(rowIds, everyone, cancellationToken);

        var sectionIds = await db.SheetSectionRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetSectionId, cancellationToken);
        await sections.ReleaseUnchangedAsync(sectionIds, everyone, cancellationToken);

        var tableIds = await db.SheetTableRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetTableId, cancellationToken);
        await tables.ReleaseUnchangedAsync(tableIds, everyone, cancellationToken);

        var columnBlockIds = await db.SheetColumnBlockRevisions.ListItemIdsWithRevisableDraftsAsync(sheetId, scope, me, revision => revision.SheetColumnBlockId, cancellationToken);
        await columnBlocks.ReleaseUnchangedAsync(columnBlockIds, everyone, cancellationToken);
    }
}
