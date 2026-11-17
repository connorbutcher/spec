using PUSpecSheet.Abstractions.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

public sealed record SubmissionReviewAccess
{
    public required SubmissionStatus Status { get; init; }

    public required int SubmittedByUserId { get; init; }

    public required bool IsApprover { get; init; }
}
