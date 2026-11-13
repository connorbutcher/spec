using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The revisions of sheet rows.</summary>
public static class SheetRowRevisionQueries
{
    /// <summary>What the user sees of the section's rows: each one's current published revision and their own draft, read only.</summary>
    public static async Task<List<DbSheetRowRevision>> ListVisibleInSectionAsync(PuSpecSheetDbContext db, int sectionId, int userId, CancellationToken cancellationToken)
    {
        return await db.SheetRowRevisions
            .Where(revision => revision.SheetRow.SheetSectionId == sectionId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>What the user sees of the rows: each one's current published revision and their own draft, read only.</summary>
    public static async Task<List<DbSheetRowRevision>> ListVisibleAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> rowIds, int userId, CancellationToken cancellationToken)
    {
        return await db.SheetRowRevisions
            .Where(revision => rowIds.Contains(revision.SheetRowId))
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>The row's current published revision and its draft, whoever holds it, read only.</summary>
    public static async Task<List<DbSheetRowRevision>> ListCurrentAndDraftAsync(PuSpecSheetDbContext db, int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == rowId)
            .CurrentAndDrafts()
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>The row's draft, tracked; null when it has none.</summary>
    public static async Task<DbSheetRowRevision?> FindDraftForEditAsync(PuSpecSheetDbContext db, int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRowRevisions
            .SingleOrDefaultAsync(
                revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft,
                cancellationToken);
    }

    /// <summary>Who holds the row's draft; null when it has none.</summary>
    public static async Task<int?> FindDraftAuthorAsync(PuSpecSheetDbContext db, int rowId, CancellationToken cancellationToken)
    {
        return await db.SheetRowRevisions
            .Where(revision => revision.SheetRowId == rowId && revision.Status == RevisionStatus.Draft)
            .Select(revision => (int?)revision.AuthorUserId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The drafts of rows in the sections that someone other than the user holds, or that wait for approval.</summary>
    public static Task<List<ContainedHold>> ListHoldsInSectionsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(db.SheetRowRevisions.Where(revision => sectionIds.Contains(revision.SheetRow.SheetSectionId)), userId, cancellationToken);
    }

    /// <summary>The drafts of the table's rows that someone other than the user holds, or that wait for approval.</summary>
    public static Task<List<ContainedHold>> ListHoldsOnTableAsync(PuSpecSheetDbContext db, int tableId, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(db.SheetRowRevisions.Where(revision => revision.SheetRow.SheetSection.SheetTableId == tableId), userId, cancellationToken);
    }
}
