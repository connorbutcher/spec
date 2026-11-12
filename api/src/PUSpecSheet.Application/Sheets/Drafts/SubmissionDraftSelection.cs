using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>The drafts waiting for approval in one submission.</summary>
public sealed record SubmissionDraftSelection : IDraftSelection
{
    public SubmissionDraftSelection(int submissionId)
    {
        this.SubmissionId = submissionId;
    }

    public int SubmissionId { get; init; }

    public IQueryable<TRevision> Apply<TRevision>(IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.InSubmission(SubmissionId);
    }
}
