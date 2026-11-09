namespace PUSpecSheet.Domain.Approvals;

/// <summary>What one approver has said about a submission.</summary>
public enum ApprovalDecision
{
    /// <summary>Not decided yet.</summary>
    Pending = 0,

    Approved = 1,

    Rejected = 2,
}
