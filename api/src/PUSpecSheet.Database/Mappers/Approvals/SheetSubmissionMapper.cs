using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.Database.Mappers.Approvals;

/// <summary>Submissions waiting for approval, or decided, as DTOs.</summary>
public static class SheetSubmissionMapper
{
    /// <summary>
    /// Expects the submission's sheet (with its phase and sheet type), its submitter, its version and its
    /// approvers (with their users) to be loaded.
    /// </summary>
    /// <param name="submission">The submission.</param>
    /// <param name="changes">How many items it changes, counted from the revisions that carry it.</param>
    public static SheetSubmissionDto ToDto(this DbSheetSubmission submission, SubmissionChangeCountsDto changes)
    {
        return new SheetSubmissionDto
        {
            Id = submission.Id,
            SheetId = submission.SheetId,
            PhaseId = submission.Sheet.PhaseId,
            PhaseCode = submission.Sheet.Phase.Code,
            SheetTypeId = submission.Sheet.SheetTypeId,
            SheetTypeName = submission.Sheet.SheetType.Name,
            Note = submission.Note,
            SubmittedByUserId = submission.SubmittedByUserId,
            SubmittedByName = submission.SubmittedBy.DisplayName,
            SubmittedAtUtc = UtcTimes.AsUtc(submission.SubmittedAtUtc),
            Status = submission.Status,
            DecidedAtUtc = UtcTimes.AsUtc(submission.DecidedAtUtc),
            VersionNumber = submission.SheetVersion?.VersionNumber,
            Changes = changes,
            Approvers = submission.Approvers
                .OrderBy(approver => approver.User.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(approver => approver.ToDto())
                .ToList(),
        };
    }

    /// <summary>Expects <see cref="DbSheetSubmissionApprover.User"/> to be loaded.</summary>
    public static SubmissionApproverDto ToDto(this DbSheetSubmissionApprover approver)
    {
        return new SubmissionApproverDto
        {
            UserId = approver.UserId,
            UserName = approver.User.DisplayName,
            Decision = approver.Decision,
            DecidedAtUtc = UtcTimes.AsUtc(approver.DecidedAtUtc),
            Reason = approver.Reason,
        };
    }
}
