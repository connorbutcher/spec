using PUSpecSheet.Abstractions.Approvals;

namespace PUSpecSheet.DataAccess.Approvals.Queries;

/// <summary>What decides whether someone may review a submission: its state, its submitter, and whether they were asked to approve it.</summary>
public sealed record SubmissionReviewAccess
{
    public required SubmissionStatus Status { get; init; }

    public required int SubmittedByUserId { get; init; }

    public required bool IsApprover { get; init; }
}
