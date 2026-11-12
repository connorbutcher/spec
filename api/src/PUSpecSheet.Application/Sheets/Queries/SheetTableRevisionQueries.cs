using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Entities.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The revisions of sheet tables.</summary>
public static class SheetTableRevisionQueries
{
    /// <summary>What the user sees of the sheet's tables: each one's current published revision and their own draft, read only.</summary>
    public static async Task<List<SheetTableRevision>> ListVisibleOnSheetAsync(PuSpecSheetDbContext db, int sheetId, int userId, CancellationToken cancellationToken)
    {
        return await db.SheetTableRevisions
            .Where(revision => revision.SheetTable.SheetId == sheetId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>The drafts that remove the table, whoever holds them.</summary>
    public static async Task<List<PendingRemoval>> ListPendingRemovalsAsync(PuSpecSheetDbContext db, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetTableRevisions
            .Where(revision => revision.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new PendingRemoval(RemovedContainer.Table, revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }
}
