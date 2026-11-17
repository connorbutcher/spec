using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Migrator.Verification;

internal static class RevisionQueries
{
    /// <summary>
    /// The revisions that show a sheet as it was at <paramref name="moment"/>: the published revision of each item whose
    /// range covers it. With no moment, the revisions that show it now with its drafts: each item's current published
    /// revision and its draft, if it has one, of which the draft is the one to show.
    /// </summary>
    public static IQueryable<TRevision> InView<TRevision>(this IQueryable<TRevision> revisions, DateTime? moment)
        where TRevision : class, IDbSheetRevision
    {
        if (moment is { } at)
        {
            return revisions.Where(revision => revision.Status == RevisionStatus.Published
                && revision.PublishedDateTime <= at
                && (revision.SupersededDateTime == null || revision.SupersededDateTime > at));
        }

        return revisions.Where(revision => revision.Status == RevisionStatus.Draft
            || (revision.Status == RevisionStatus.Published && revision.SupersededDateTime == null));
    }

    /// <summary>
    /// The revision to show for each item, of those in view: its draft where it has one alongside its published revision.
    /// </summary>
    public static Dictionary<int, T> ShownPerItem<T>(this IEnumerable<T> revisions, Func<T, int> itemId, Func<T, RevisionStatus> status)
    {
        return revisions
            .GroupBy(itemId)
            .ToDictionary(item => item.Key, item => item.OrderBy(revision => status(revision) == RevisionStatus.Draft ? 0 : 1).First());
    }
}
