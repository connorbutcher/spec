namespace PUSpecSheet.Abstractions.Approvals;

/// <summary>One of a submission's approvers and what they decided; <see cref="Reason"/> is set for a rejection.</summary>
public sealed record SubmissionApprover
{
    public required int UserId { get; init; }

    public required string UserName { get; init; }

    public required ApprovalDecision Decision { get; init; }

    public DateTime? DecidedAtUtc { get; init; }

    public string? Reason { get; init; }
}
