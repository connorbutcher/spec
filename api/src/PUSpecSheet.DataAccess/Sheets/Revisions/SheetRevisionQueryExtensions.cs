using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Revisions;

public static class SheetRevisionQueryExtensions
{
    public static IQueryable<TRevision> Current<TRevision>(this IQueryable<TRevision> revisions)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published && revision.SupersededAtUtc == null);
    }

    public static IQueryable<TRevision> Drafts<TRevision>(this IQueryable<TRevision> revisions)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Draft);
    }

    public static IQueryable<TRevision> DraftsIn<TRevision>(this IQueryable<TRevision> revisions, PublishScope scope, int publisherUserId)
        where TRevision : class, IDbSheetRevision
    {
        var drafts = scope == PublishScope.All ? revisions.Drafts() : revisions.DraftsOf(publisherUserId);
        return drafts.Where(revision => revision.SheetSubmissionId == null);
    }

    public static IQueryable<TRevision> InSubmission<TRevision>(this IQueryable<TRevision> revisions, int submissionId)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Draft && revision.SheetSubmissionId == submissionId);
    }

    public static IQueryable<TRevision> DraftsOf<TRevision>(this IQueryable<TRevision> revisions, int userId)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Draft && revision.AuthorUserId == userId);
    }

    public static IQueryable<TRevision> CurrentAndDrafts<TRevision>(this IQueryable<TRevision> revisions)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Current()
            .Concat(revisions.Drafts());
    }

    public static IQueryable<TRevision> InView<TRevision>(this IQueryable<TRevision> revisions, DateTime? moment)
        where TRevision : class, IDbSheetRevision
    {
        return moment is { } at
            ? revisions.Where(revision => revision.Status == RevisionStatus.Published
                && revision.PublishedAtUtc <= at
                && (revision.SupersededAtUtc == null || revision.SupersededAtUtc > at))
            : revisions.CurrentAndDrafts();
    }

    public static IQueryable<TRevision> PublishedUpTo<TRevision>(this IQueryable<TRevision> revisions, DateTime? moment)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Where(revision => revision.Status == RevisionStatus.Published
            && (moment == null || revision.PublishedAtUtc <= moment));
    }

    public static IQueryable<TRevision> VisibleTo<TRevision>(this IQueryable<TRevision> revisions, int userId)
        where TRevision : class, IDbSheetRevision
    {
        return revisions.Current()
            .Concat(revisions.DraftsOf(userId));
    }
}
