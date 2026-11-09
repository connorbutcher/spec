using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Contracts.Approvals;

/// <summary>One of a submission's approvers and what they decided; <paramref name="Reason"/> is set for a rejection.</summary>
public sealed record SubmissionApproverDto(
    int UserId,
    string UserName,
    ApprovalDecision Decision,
    DateTime? DecidedAtUtc,
    string? Reason);
