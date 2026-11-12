using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Revisions;

/// <summary>
/// The live revisions of sheet items: the ones that haven't been replaced. An item has at most one current
/// published revision and at most one draft, each guaranteed by a filtered unique index, and these queries
/// are written as those two cases so SQL Server answers them from those indexes. Asking only for
/// "not superseded" matches neither index, and reads every revision the items have ever had.
/// </summary>
public static class SheetRevisionQueryExtensions
{
    /// <summary>Each item's current published revision.</summary>
    public static IQueryable<TRevision> Current<TRevision>(this IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null);
    }

    /// <summary>Every draft, whoever holds it.</summary>
    public static IQueryable<TRevision> Drafts<TRevision>(this IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Draft);
    }

    /// <summary>
    /// The drafts a publish takes: the publisher's own, or everyone's. Drafts already waiting for approval
    /// are left out, as they belong to their submission until it is decided.
    /// </summary>
    public static IQueryable<TRevision> DraftsIn<TRevision>(this IQueryable<TRevision> revisions, PublishScope scope, int publisherUserId)
        where TRevision : class, ISheetRevision
    {
        var drafts = scope == PublishScope.All ? revisions.Drafts() : revisions.DraftsOf(publisherUserId);
        return drafts.Where(revision => revision.SheetSubmissionId == null);
    }

    /// <summary>The drafts one user holds that aren't waiting for approval, so they can still change or discard them.</summary>
    public static IQueryable<TRevision> OpenDraftsOf<TRevision>(this IQueryable<TRevision> revisions, int userId)
        where TRevision : class, ISheetRevision
    {
        return revisions.DraftsOf(userId).Where(revision => revision.SheetSubmissionId == null);
    }

    /// <summary>The drafts waiting for approval in one submission.</summary>
    public static IQueryable<TRevision> InSubmission<TRevision>(this IQueryable<TRevision> revisions, int submissionId)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Draft && revision.SheetSubmissionId == submissionId);
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
            .Concat(revisions.Drafts());
    }

    /// <summary>
    /// The revisions a view can show. The live view (no moment) needs every revision that hasn't been
    /// replaced, which is the current published one and any drafts. A past view needs the one published
    /// revision whose range covers the moment, which is what the (item, published, superseded) indexes are for.
    /// </summary>
    public static IQueryable<TRevision> InView<TRevision>(this IQueryable<TRevision> revisions, DateTime? moment)
        where TRevision : class, ISheetRevision
    {
        return moment is { } at
            ? revisions.Where(revision => revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= at
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > at))
            : revisions.CurrentAndDrafts();
    }

    /// <summary>Every revision published up to the moment, or ever when there is none.</summary>
    public static IQueryable<TRevision> PublishedUpTo<TRevision>(this IQueryable<TRevision> revisions, DateTime? moment)
        where TRevision : class, ISheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published
            && (moment == null || revision.PublishedAtUtc <= moment));
    }

    /// <summary>What a user sees of each item: its current published revision and their own draft of it.</summary>
    public static IQueryable<TRevision> VisibleTo<TRevision>(this IQueryable<TRevision> revisions, int userId)
        where TRevision : class, ISheetRevision
    {
        return revisions.Current()
            .Concat(revisions.DraftsOf(userId));
    }
}
