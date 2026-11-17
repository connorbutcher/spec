using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database.Schema.Access;

namespace PUSpecSheet.Api.Database.Schema.Approval;

/// <summary>
/// An approver of a submission and their decision.
/// </summary>
public record DbSheetSubmissionApprover
{
    public required int SheetSubmissionId { get; init; }

    public required int UserId { get; init; }

    public required ApproverDecision Decision { get; set; }

    public required DateTime? DecidedDateTime { get; set; }

    public required string? Reason { get; set; }

    public virtual DbUser? User { get; init; }
}
