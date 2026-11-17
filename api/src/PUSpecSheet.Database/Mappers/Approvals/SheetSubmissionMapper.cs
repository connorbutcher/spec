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
        return new SheetSubmissionDto(
            submission.Id,
            submission.SheetId,
            submission.Sheet.PhaseId,
            submission.Sheet.Phase.Code,
            submission.Sheet.SheetTypeId,
            submission.Sheet.SheetType.Name,
            submission.Note,
            submission.SubmittedByUserId,
            submission.SubmittedBy.DisplayName,
            UtcTimes.AsUtc(submission.SubmittedAtUtc),
            submission.Status,
            UtcTimes.AsUtc(submission.DecidedAtUtc),
            submission.SheetVersion?.VersionNumber,
            changes,
            submission.Approvers
                .OrderBy(approver => approver.User.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(approver => approver.ToDto())
                .ToList());
    }

    /// <summary>Expects <see cref="DbSheetSubmissionApprover.User"/> to be loaded.</summary>
    public static SubmissionApproverDto ToDto(this DbSheetSubmissionApprover approver)
    {
        return new SubmissionApproverDto(
            approver.UserId,
            approver.User.DisplayName,
            approver.Decision,
            UtcTimes.AsUtc(approver.DecidedAtUtc),
            approver.Reason);
    }
}
