using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The revisions of sheet column blocks.</summary>
public static class SheetColumnBlockRevisionQueries
{
    /// <summary>What the user sees of the table's column blocks: each one's current published revision and their own draft, read only.</summary>
    public static async Task<List<DbSheetColumnBlockRevision>> ListVisibleOnTableAsync(PuSpecSheetDbContext db, int tableId, int userId, CancellationToken cancellationToken)
    {
        return await db.SheetColumnBlockRevisions
            .Where(revision => revision.SheetColumnBlock.SheetTableId == tableId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>The drafts of the table's column blocks that someone other than the user holds, or that wait for approval.</summary>
    public static Task<List<ContainedHold>> ListHoldsOnTableAsync(PuSpecSheetDbContext db, int tableId, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(db.SheetColumnBlockRevisions.Where(revision => revision.SheetColumnBlock.SheetTableId == tableId), userId, cancellationToken);
    }
}
