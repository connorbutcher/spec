namespace PUSpecSheet.Abstractions.Approvals;

/// <summary>
/// A publish waiting for approval, or one that was decided. <see cref="Changes"/> counts what it
/// changes; <see cref="VersionNumber"/> is the version an approval published.
/// </summary>
public sealed record SheetSubmissionDto
{
    public SheetSubmissionDto(
        int id,
        int sheetId,
        int phaseId,
        string phaseCode,
        int sheetTypeId,
        string sheetTypeName,
        string? note,
        int submittedByUserId,
        string submittedByName,
        DateTime submittedAtUtc,
        SubmissionStatus status,
        DateTime? decidedAtUtc,
        int? versionNumber,
        SubmissionChangeCountsDto changes,
        IReadOnlyList<SubmissionApproverDto> approvers)
    {
        Id = id;
        SheetId = sheetId;
        PhaseId = phaseId;
        PhaseCode = phaseCode;
        SheetTypeId = sheetTypeId;
        SheetTypeName = sheetTypeName;
        Note = note;
        SubmittedByUserId = submittedByUserId;
        SubmittedByName = submittedByName;
        SubmittedAtUtc = submittedAtUtc;
        Status = status;
        DecidedAtUtc = decidedAtUtc;
        VersionNumber = versionNumber;
        Changes = changes;
        Approvers = approvers;
    }

    public int Id { get; init; }

    public int SheetId { get; init; }

    public int PhaseId { get; init; }

    public string PhaseCode { get; init; }

    public int SheetTypeId { get; init; }

    public string SheetTypeName { get; init; }

    public string? Note { get; init; }

    public int SubmittedByUserId { get; init; }

    public string SubmittedByName { get; init; }

    public DateTime SubmittedAtUtc { get; init; }

    public SubmissionStatus Status { get; init; }

    public DateTime? DecidedAtUtc { get; init; }

    public int? VersionNumber { get; init; }

    public SubmissionChangeCountsDto Changes { get; init; }

    public IReadOnlyList<SubmissionApproverDto> Approvers { get; init; }
}
