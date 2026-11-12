using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Application.Approvals.Queries;

/// <summary>What decides whether someone may review a submission: its state, its submitter, and whether they were asked to approve it.</summary>
public sealed record SubmissionReviewAccess
{
    public SubmissionReviewAccess(SubmissionStatus status, int submittedByUserId, bool isApprover)
    {
        this.Status = status;
        this.SubmittedByUserId = submittedByUserId;
        this.IsApprover = isApprover;
    }

    public SubmissionStatus Status { get; init; }

    public int SubmittedByUserId { get; init; }

    public bool IsApprover { get; init; }
}
