using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Api.Database.Schema.Spec;

public static class DbSheetRevisionExtensions
{
    /// <summary>
    /// Of the revisions read for one item, the one to show: its draft where it has one, otherwise its published revision.
    /// </summary>
    public static TRevision Shown<TRevision>(this IEnumerable<TRevision> revisions)
        where TRevision : IDbSheetRevision
    {
        ArgumentNullException.ThrowIfNull(revisions, nameof(revisions));

        return revisions.OrderBy(revision => revision.Status == RevisionStatus.Draft ? 0 : 1).First();
    }

    /// <summary>
    /// Whether the item is on the sheet: the revision to show doesn't remove it.
    /// </summary>
    public static bool IsShown<TRevision>(this IEnumerable<TRevision> revisions)
        where TRevision : IDbSheetRevision
    {
        return !revisions.Shown().IsDeleted;
    }
}
