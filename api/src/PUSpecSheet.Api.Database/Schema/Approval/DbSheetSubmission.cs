using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Approval;

/// <summary>
/// A sheet's drafts sent for approval. The revisions it holds point at it; an approved one names the version it published.
/// </summary>
public record DbSheetSubmission
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required string? Note { get; init; }

    public required int SubmittedByUserId { get; init; }

    public required DateTime SubmittedDateTime { get; init; }

    public required SubmissionStatus Status { get; set; }

    public required DateTime? DecidedDateTime { get; set; }

    public required int? SheetVersionId { get; set; }

    public byte[] RowVersion { get; init; } = [];

    public virtual DbUser? SubmittedByUser { get; init; }

    public virtual ICollection<DbSheetSubmissionApprover> Approvers { get; init; } = [];
}
