using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Drops the current user's drafts on a sheet that make no difference to what is published, such as a row
/// locked or edited and then put back. Normally a draft is released the moment it is put back; this catches
/// ones left from before that, so they don't light up Publish or hold a lock for nothing.
/// </summary>
public sealed class DraftSweeper(
    PuSpecSheetDbContext db,
    ICurrentUser currentUser,
    TableDrafts tables,
    SectionDrafts sections,
    RowDrafts rows,
    ColumnBlockDrafts columnBlocks)
{
    public async Task SweepAsync(int sheetId, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;

        // Only an item with an earlier revision has something to return to.
        var rowIds = await db.SheetRowRevisions
            .Where(revision => revision.SheetRow.SheetSection.SheetTable.SheetId == sheetId
                && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me && revision.RevisionNumber > 1)
            .Select(revision => revision.SheetRowId)
            .ToListAsync(cancellationToken);
        await rows.ReleaseUnchangedAsync(rowIds, cancellationToken);

        var sectionIds = await db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTable.SheetId == sheetId
                && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me && revision.RevisionNumber > 1)
            .Select(revision => revision.SheetSectionId)
            .ToListAsync(cancellationToken);
        await sections.ReleaseUnchangedAsync(sectionIds, cancellationToken);

        var tableIds = await db.SheetTableRevisions
            .Where(revision => revision.SheetTable.SheetId == sheetId
                && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me && revision.RevisionNumber > 1)
            .Select(revision => revision.SheetTableId)
            .ToListAsync(cancellationToken);
        await tables.ReleaseUnchangedAsync(tableIds, cancellationToken);

        var blockIds = await db.SheetColumnBlockRevisions
            .Where(revision => revision.SheetColumnBlock.SheetTable.SheetId == sheetId
                && revision.Status == RevisionStatus.Draft && revision.AuthorUserId == me && revision.RevisionNumber > 1)
            .Select(revision => revision.SheetColumnBlockId)
            .ToListAsync(cancellationToken);
        await columnBlocks.ReleaseUnchangedAsync(blockIds, cancellationToken);
    }
}
