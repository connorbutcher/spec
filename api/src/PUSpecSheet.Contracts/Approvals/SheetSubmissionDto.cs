using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Contracts.Approvals;

/// <summary>
/// A publish waiting for approval, or one that was decided. <paramref name="Changes"/> counts what it
/// changes; <paramref name="VersionNumber"/> is the version an approval published.
/// </summary>
public sealed record SheetSubmissionDto(
    int Id,
    int SheetId,
    int PhaseId,
    string PhaseCode,
    int SheetTypeId,
    string SheetTypeName,
    string? Note,
    int SubmittedByUserId,
    string SubmittedByName,
    DateTime SubmittedAtUtc,
    SubmissionStatus Status,
    DateTime? DecidedAtUtc,
    int? VersionNumber,
    SubmissionChangeCountsDto Changes,
    IReadOnlyList<SubmissionApproverDto> Approvers);
