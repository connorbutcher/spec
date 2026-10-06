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

        var rowIds = await HavingAnEarlierRevision(db.RowRevisionsOf(sheetId).DraftsOf(me))
            .Select(revision => revision.SheetRowId)
            .ToListAsync(cancellationToken);
        await rows.ReleaseUnchangedAsync(rowIds, cancellationToken);

        var sectionIds = await HavingAnEarlierRevision(db.SectionRevisionsOf(sheetId).DraftsOf(me))
            .Select(revision => revision.SheetSectionId)
            .ToListAsync(cancellationToken);
        await sections.ReleaseUnchangedAsync(sectionIds, cancellationToken);

        var tableIds = await HavingAnEarlierRevision(db.TableRevisionsOf(sheetId).DraftsOf(me))
            .Select(revision => revision.SheetTableId)
            .ToListAsync(cancellationToken);
        await tables.ReleaseUnchangedAsync(tableIds, cancellationToken);

        var columnBlockIds = await HavingAnEarlierRevision(db.ColumnBlockRevisionsOf(sheetId).DraftsOf(me))
            .Select(revision => revision.SheetColumnBlockId)
            .ToListAsync(cancellationToken);
        await columnBlocks.ReleaseUnchangedAsync(columnBlockIds, cancellationToken);
    }

    /// <summary>Only an item with an earlier revision has something to return to.</summary>
    private static IQueryable<TRevision> HavingAnEarlierRevision<TRevision>(IQueryable<TRevision> drafts)
        where TRevision : class, ISheetRevision
    {
        return drafts.Where(revision => revision.RevisionNumber > 1);
    }
}
