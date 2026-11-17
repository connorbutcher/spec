namespace PUSpecSheet.Abstractions.Approvals;

/// <summary>
/// A publish waiting for approval, or one that was decided. <see cref="Changes"/> counts what it
/// changes; <see cref="VersionNumber"/> is the version an approval published.
/// </summary>
public sealed record SheetSubmissionDto
{
    public required int Id { get; init; }

    public required int SheetId { get; init; }

    public required int PhaseId { get; init; }

    public required string PhaseCode { get; init; }

    public required int SheetTypeId { get; init; }

    public required string SheetTypeName { get; init; }

    public string? Note { get; init; }

    public required int SubmittedByUserId { get; init; }

    public required string SubmittedByName { get; init; }

    public required DateTime SubmittedAtUtc { get; init; }

    public required SubmissionStatus Status { get; init; }

    public DateTime? DecidedAtUtc { get; init; }

    public int? VersionNumber { get; init; }

    public required SubmissionChangeCountsDto Changes { get; init; }

    public required IReadOnlyList<SubmissionApproverDto> Approvers { get; init; }
}
