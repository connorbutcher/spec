using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

public record SheetSubmissionApprover
{
    public required int SheetSubmissionId { get; init; }

    public required int UserId { get; init; }

    public User? User { get; init; }

    public required ApproverDecision Decision { get; init; }

    public required DateTime? DecidedDateTime { get; init; }

    /// <summary>
    /// Why the approver decided as they did; null when they gave no reason.
    /// </summary>
    public required string? Reason { get; init; }
}
