using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Approvals;
using PUSpecSheet.Application.Sheets.Publishing;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Database.Entities.Approvals;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Application.Approvals;

/// <summary>
/// Submits drafts for publishing. The drafts a publish would take are checked as a publish checks them,
/// then tagged with a new pending submission: they stay drafts, so no published view shows them, and stay
/// locked to their authors, who can't change them until the submission is decided.
/// </summary>
public sealed class SheetSubmitter
{
    private readonly ISheetSubmissionRepository submissions;
    private readonly IUnitOfWork unitOfWork;
    private readonly SheetPublisher publisher;
    private readonly ApproverDirectory approvers;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider clock;

    public SheetSubmitter(
        ISheetSubmissionRepository submissions,
        IUnitOfWork unitOfWork,
        SheetPublisher publisher,
        ApproverDirectory approvers,
        ICurrentUser currentUser,
        TimeProvider clock)
    {
        this.submissions = submissions;
        this.unitOfWork = unitOfWork;
        this.publisher = publisher;
        this.approvers = approvers;
        this.currentUser = currentUser;
        this.clock = clock;
    }

    /// <returns>The new submission's id.</returns>
    public async Task<int> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        var me = currentUser.UserId;
        var approverIds = await approvers.EnsureEligibleAsync(request.ApproverUserIds, me, cancellationToken);
        var drafts = await publisher.PrepareAsync(sheetId, request.Scope, cancellationToken);

        var submission = new SheetSubmission
        {
            SheetId = sheetId,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            SubmittedByUserId = me,
            SubmittedAtUtc = clock.GetUtcNow().UtcDateTime,
            Status = SubmissionStatus.Pending,
        };
        foreach (var approverId in approverIds)
        {
            submission.Approvers.Add(new SheetSubmissionApprover { UserId = approverId, Decision = ApprovalDecision.Pending });
        }

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);
        submissions.Add(submission);
        await unitOfWork.SaveSheetChangesAsync(cancellationToken);

        // The drafts' row versions make this fail, as a conflict, if an author saves into one at the same moment.
        foreach (var draft in drafts.All)
        {
            draft.SheetSubmissionId = submission.Id;
        }

        await unitOfWork.SaveSheetChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return submission.Id;
    }
}
