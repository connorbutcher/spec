using PUSpecSheet.Application.Sheets.Revisions;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Application.Sheets.Drafts;

/// <summary>The drafts waiting for approval in one submission.</summary>
public sealed record SubmissionDraftSelection(int SubmissionId) : IDraftSelection
{
    public IQueryable<TRevision> Apply<TRevision>(IQueryable<TRevision> revisions)
        where TRevision : class, ISheetRevision
    {
        return revisions.InSubmission(SubmissionId);
    }
}
