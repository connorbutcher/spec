using PUSpecSheet.Database.Models.Users;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Database.Models.Approvals;

/// <summary>One person the submitter asked to approve a submission, and what they decided.</summary>
public class DbSheetSubmissionApprover
{
    public int SheetSubmissionId { get; set; }

    public DbSheetSubmission SheetSubmission { get; set; } = null!;

    public int UserId { get; set; }

    public DbUser User { get; set; } = null!;

    public ApprovalDecision Decision { get; set; }

    public DateTime? DecidedAtUtc { get; set; }

    /// <summary>Why they rejected it; required for a rejection.</summary>
    public string? Reason { get; set; }
}
