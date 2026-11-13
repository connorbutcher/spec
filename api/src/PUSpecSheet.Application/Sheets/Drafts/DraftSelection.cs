using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>
/// Which of a sheet's drafts a publish takes, written once for every kind of revision: the drafts in a publish
/// scope that aren't already waiting for approval, or the drafts waiting for approval in one submission.
/// </summary>
public sealed class DraftSelection
{
    private readonly PublishScope scope;
    private readonly int publisherUserId;
    private readonly int? submissionId;

    private DraftSelection(PublishScope scope, int publisherUserId, int? submissionId)
    {
        this.scope = scope;
        this.publisherUserId = publisherUserId;
        this.submissionId = submissionId;
    }

    /// <summary>The drafts in a publish scope (the publisher's own or everyone's) that aren't already waiting for approval.</summary>
    public static DraftSelection InScope(PublishScope scope, int publisherUserId)
    {
        return new DraftSelection(scope, publisherUserId, submissionId: null);
    }

    /// <summary>The drafts waiting for approval in one submission.</summary>
    public static DraftSelection InSubmission(int submissionId)
    {
        return new DraftSelection(PublishScope.Mine, publisherUserId: 0, submissionId);
    }

    public IQueryable<TRevision> Apply<TRevision>(IQueryable<TRevision> revisions)
        where TRevision : class, IDbSheetRevision
    {
        return submissionId is { } id
            ? revisions.InSubmission(id)
            : revisions.DraftsIn(scope, publisherUserId);
    }
}
