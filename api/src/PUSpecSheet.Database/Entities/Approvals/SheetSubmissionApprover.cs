using PUSpecSheet.Database.Entities.Users;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Database.Entities.Approvals;

/// <summary>One person the submitter asked to approve a submission, and what they decided.</summary>
public class SheetSubmissionApprover
{
    public int SheetSubmissionId { get; set; }

    public SheetSubmission SheetSubmission { get; set; } = null!;

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public ApprovalDecision Decision { get; set; }

    public DateTime? DecidedAtUtc { get; set; }

    /// <summary>Why they rejected it; required for a rejection.</summary>
    public string? Reason { get; set; }
}
