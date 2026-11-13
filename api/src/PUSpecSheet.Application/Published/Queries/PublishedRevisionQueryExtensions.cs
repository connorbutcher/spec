using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Database;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Published.Queries;

public static class PublishedRevisionQueryExtensions
{
    /// <summary>
    /// The published revisions whose <c>[PublishedAtUtc, SupersededAtUtc)</c> range covers a moment: one
    /// per item, found through the (item, published) index.
    /// </summary>
    public static IQueryable<TRevision> PublishedAt<TRevision>(this IQueryable<TRevision> revisions, DateTime momentUtc)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published
            && revision.PublishedAtUtc <= momentUtc
            && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > momentUtc));
    }

    /// <summary>
    /// The ids of the rows' revisions shown at a version, found through the (sheet, published) index: the
    /// same revisions the published cell queries join to.
    /// </summary>
    public static IQueryable<int> RowRevisionIdsAt(this PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        var sheetId = version.SheetId;
        return db.SheetRowRevisions
            .Where(revision => revision.SheetId == sheetId && !revision.IsDeleted)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => revision.Id);
    }
}
