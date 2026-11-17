using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;

internal static class RevisionQueries
{
    /// <summary>
    /// Each item's published revision as it stood at <paramref name="moment"/>: published at or before it, and not yet
    /// replaced.
    /// </summary>
    public static IQueryable<TRevision> PublishedAt<TRevision>(this IQueryable<TRevision> revisions, DateTime moment)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published
            && revision.PublishedDateTime <= moment
            && (revision.SupersededDateTime == null || revision.SupersededDateTime > moment));
    }
}
