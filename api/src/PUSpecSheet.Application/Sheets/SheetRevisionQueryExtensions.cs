using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets;

/// <summary>
/// The live revisions of sheet items: the ones that haven't been replaced. An item has at most one current
/// published revision and at most one draft, each guaranteed by a filtered unique index, and these queries
/// are written as those two cases so SQL Server answers them from those indexes. Asking only for
/// "not superseded" matches neither index, and reads every revision the items have ever had.
/// </summary>
internal static class SheetRevisionQueryExtensions
{
    /// <summary>Each item's current published revision.</summary>
    public static IQueryable<TRevision> Current<TRevision>(this IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null);
    }

    /// <summary>The drafts one user holds.</summary>
    public static IQueryable<TRevision> DraftsOf<TRevision>(this IQueryable<TRevision> revisions, int userId)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Draft && revision.AuthorUserId == userId);
    }

    /// <summary>Each item's current published revision and its draft, whoever holds it.</summary>
    public static IQueryable<TRevision> CurrentAndDrafts<TRevision>(this IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.Current()
            .Concat(revisions.Where(revision => revision.Status == RevisionStatus.Draft));
    }

    /// <summary>What a user sees of each item: its current published revision and their own draft of it.</summary>
    public static IQueryable<TRevision> VisibleTo<TRevision>(this IQueryable<TRevision> revisions, int userId)
        where TRevision : class, ISheetRevision
    {
        return revisions.Current()
            .Concat(revisions.DraftsOf(userId));
    }
}
