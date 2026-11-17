using PUSpecSheet.Abstractions.Approvals;

namespace PUSpecSheet.DataAccess.Approvals.Queries;

/// <summary>What decides whether someone may review a submission: its state, its submitter, and whether they were asked to approve it.</summary>
public sealed record SubmissionReviewAccess
{
    public SubmissionReviewAccess(SubmissionStatus status, int submittedByUserId, bool isApprover)
    {
        Status = status;
        SubmittedByUserId = submittedByUserId;
        IsApprover = isApprover;
    }

    public SubmissionStatus Status { get; init; }

    public int SubmittedByUserId { get; init; }

    public bool IsApprover { get; init; }
}
