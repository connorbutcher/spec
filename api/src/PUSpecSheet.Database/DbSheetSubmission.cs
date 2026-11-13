using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Database;

/// <summary>
/// A publish waiting for approval. Its drafts stay drafts, tagged with the submission, so nothing about
/// them is published until every approver has approved; then they become the sheet's next version.
/// </summary>
public class DbSheetSubmission
{
    public int Id { get; set; }

    public int SheetId { get; set; }

    public DbSheet Sheet { get; set; } = null!;

    /// <summary>The note the version will carry once approved.</summary>
    public string? Note { get; set; }

    public int SubmittedByUserId { get; set; }

    public DbUser SubmittedBy { get; set; } = null!;

    public DateTime SubmittedAtUtc { get; set; }

    public SubmissionStatus Status { get; set; }

    /// <summary>When it was approved, rejected or withdrawn.</summary>
    public DateTime? DecidedAtUtc { get; set; }

    /// <summary>The version the approval published.</summary>
    public int? SheetVersionId { get; set; }

    public DbSheetVersion? SheetVersion { get; set; }

    /// <summary>Concurrency token: two approvers deciding at the same moment can't both finish it.</summary>
    public byte[] RowVersion { get; set; } = [];

    public ICollection<DbSheetSubmissionApprover> Approvers { get; set; } = [];
}
