namespace PUSpecSheet.Abstractions.Approvals;

/// <summary>One of a submission's approvers and what they decided; <see cref="Reason"/> is set for a rejection.</summary>
public sealed record SubmissionApproverDto
{
    public SubmissionApproverDto(int userId, string userName, ApprovalDecision decision, DateTime? decidedAtUtc, string? reason)
    {
        UserId = userId;
        UserName = userName;
        Decision = decision;
        DecidedAtUtc = decidedAtUtc;
        Reason = reason;
    }

    public int UserId { get; init; }

    public string UserName { get; init; }

    public ApprovalDecision Decision { get; init; }

    public DateTime? DecidedAtUtc { get; init; }

    public string? Reason { get; init; }
}
