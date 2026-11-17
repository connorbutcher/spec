using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.DataAccess.Sheets.Revisions;
using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.DataAccess.Sheets.Drafts;

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

    public static DraftSelection InScope(PublishScope scope, int publisherUserId)
    {
        return new DraftSelection(scope, publisherUserId, submissionId: null);
    }

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
