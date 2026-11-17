using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

/// <summary>
/// Which of a sheet's drafts a publish takes, written once for every kind of revision: the drafts in a publish
/// scope that aren't already waiting for approval, or the drafts waiting for approval in one submission.
/// </summary>
public sealed class DraftSelection
{
    private readonly PublishScope _scope;
    private readonly int _publisherUserId;
    private readonly int? _submissionId;

    private DraftSelection(PublishScope scope, int publisherUserId, int? submissionId)
    {
        _scope = scope;
        _publisherUserId = publisherUserId;
        _submissionId = submissionId;
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
        return _submissionId is { } id
            ? revisions.InSubmission(id)
            : revisions.DraftsIn(_scope, _publisherUserId);
    }
}
