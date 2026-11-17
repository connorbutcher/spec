using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Published.Queries;

public static class PublishedRevisionQueryExtensions
{
    public static IQueryable<TRevision> PublishedAt<TRevision>(this IQueryable<TRevision> revisions, DateTime momentUtc)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published
            && revision.PublishedAtUtc <= momentUtc
            && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > momentUtc));
    }

    public static IQueryable<int> RowRevisionIdsAt(this PuSpecSheetDbContext db, ResolvedSheetVersion version)
    {
        var sheetId = version.SheetId;
        return db.SheetRowRevisions
            .Where(revision => revision.SheetId == sheetId && !revision.IsDeleted)
            .PublishedAt(version.PublishedAtUtc)
            .Select(revision => revision.Id);
    }
}
