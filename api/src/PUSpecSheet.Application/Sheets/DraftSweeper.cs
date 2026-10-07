using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// Drops the drafts on a sheet that make no difference to what is published, such as a row locked or edited
/// and then put back. Normally a draft is released the moment it is put back; this catches ones left from
/// before that, so they don't light up Publish or hold a lock for nothing. It takes the current user's
/// drafts, or everyone's ahead of a publish of everything.
/// </summary>
public sealed class DraftSweeper(
    PuSpecSheetDbContext db,
    ICurrentUser currentUser,
    TableDrafts tables,
    SectionDrafts sections,
    RowDrafts rows,
    ColumnBlockDrafts columnBlocks)
{
    public Task SweepAsync(int sheetId, CancellationToken cancellationToken)
    {
        return SweepAsync(sheetId, PublishScope.Mine, cancellationToken);
    }

    public async Task SweepAsync(int sheetId, PublishScope scope, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var everyone = scope == PublishScope.All;

        var rowIds = await HavingAnEarlierRevision(db.RowRevisionsOf(sheetId).DraftsIn(scope, me))
            .Select(revision => revision.SheetRowId)
            .ToListAsync(cancellationToken);
        await rows.ReleaseUnchangedAsync(rowIds, everyone, cancellationToken);

        var sectionIds = await HavingAnEarlierRevision(db.SectionRevisionsOf(sheetId).DraftsIn(scope, me))
            .Select(revision => revision.SheetSectionId)
            .ToListAsync(cancellationToken);
        await sections.ReleaseUnchangedAsync(sectionIds, everyone, cancellationToken);

        var tableIds = await HavingAnEarlierRevision(db.TableRevisionsOf(sheetId).DraftsIn(scope, me))
            .Select(revision => revision.SheetTableId)
            .ToListAsync(cancellationToken);
        await tables.ReleaseUnchangedAsync(tableIds, everyone, cancellationToken);

        var columnBlockIds = await HavingAnEarlierRevision(db.ColumnBlockRevisionsOf(sheetId).DraftsIn(scope, me))
            .Select(revision => revision.SheetColumnBlockId)
            .ToListAsync(cancellationToken);
        await columnBlocks.ReleaseUnchangedAsync(columnBlockIds, everyone, cancellationToken);
    }

    /// <summary>Only an item with an earlier revision has something to return to.</summary>
    private static IQueryable<TRevision> HavingAnEarlierRevision<TRevision>(IQueryable<TRevision> drafts)
        where TRevision : class, ISheetRevision
    {
        return drafts.Where(revision => revision.RevisionNumber > 1);
    }
}
