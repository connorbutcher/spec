using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Published;

public static class PublishedRevisionQueryExtensions
{
    /// <summary>
    /// The published revisions whose <c>[PublishedAtUtc, SupersededAtUtc)</c> range covers a moment: one
    /// per item, found through the (item, published) index.
    /// </summary>
    public static IQueryable<TRevision> PublishedAt<TRevision>(this IQueryable<TRevision> revisions, DateTime momentUtc)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published
            && revision.PublishedAtUtc <= momentUtc
            && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > momentUtc));
    }
}
