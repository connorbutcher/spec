using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Types;

namespace PUSpecSheet.Abstractions.Domain.Sheets;

/// <summary>
/// A request to approve the sheet's drafts, which publishes them as a version when approved.
/// </summary>
public record SheetSubmission
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required string? Note { get; init; }

    public required int SubmittedByUserId { get; init; }

    public User? SubmittedByUser { get; init; }

    public required DateTime SubmittedDateTime { get; init; }

    public required SubmissionStatus Status { get; init; }

    public required DateTime? DecidedDateTime { get; init; }

    /// <summary>
    /// The version the submission published, once it has.
    /// </summary>
    public required int? SheetVersionId { get; init; }

    public required int? SheetVersionNumber { get; init; }

    public required ICollection<SheetSubmissionApprover> Approvers { get; init; } = [];
}
