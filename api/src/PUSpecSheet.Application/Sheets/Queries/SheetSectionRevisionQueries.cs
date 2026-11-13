using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Queries;

/// <summary>The revisions of sheet sections.</summary>
public static class SheetSectionRevisionQueries
{
    /// <summary>What the user sees of the table's sections: each one's current published revision and their own draft, read only.</summary>
    public static async Task<List<DbSheetSectionRevision>> ListVisibleOnTableAsync(PuSpecSheetDbContext db, int tableId, int userId, CancellationToken cancellationToken)
    {
        return await db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId)
            .VisibleTo(userId)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    /// <summary>The drafts that remove a section of the table, whoever holds them.</summary>
    public static async Task<List<SectionRemoval>> ListPendingRemovalsOnTableAsync(PuSpecSheetDbContext db, int tableId, CancellationToken cancellationToken)
    {
        return await db.SheetSectionRevisions
            .Where(revision => revision.SheetSection.SheetTableId == tableId && revision.Status == RevisionStatus.Draft && revision.IsDeleted)
            .Select(revision => new SectionRemoval(revision.SheetSectionId, revision.AuthorUserId, revision.SheetSubmissionId != null))
            .ToListAsync(cancellationToken);
    }

    /// <summary>The drafts of the sections that someone other than the user holds, or that wait for approval.</summary>
    public static Task<List<ContainedHold>> ListHoldsAsync(PuSpecSheetDbContext db, IReadOnlyCollection<int> sectionIds, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(db.SheetSectionRevisions.Where(revision => sectionIds.Contains(revision.SheetSectionId)), userId, cancellationToken);
    }

    /// <summary>The drafts of the table's sections that someone other than the user holds, or that wait for approval.</summary>
    public static Task<List<ContainedHold>> ListHoldsOnTableAsync(PuSpecSheetDbContext db, int tableId, int userId, CancellationToken cancellationToken)
    {
        return SheetRevisionQueries.ListHoldsAsync(db.SheetSectionRevisions.Where(revision => revision.SheetSection.SheetTableId == tableId), userId, cancellationToken);
    }
}
