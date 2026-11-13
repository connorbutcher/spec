using PUSpecSheet.Application.Approvals.Queries;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Sheets.Publishing;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Application.Approvals;

/// <summary>
/// Records approvers' decisions on a pending submission. Every chosen approver has to approve; the last
/// approval publishes the drafts as the sheet's next version. A single rejection, which must say why,
/// rejects it at once; the submitter can also withdraw it while it waits. Either way the drafts go back to
/// their authors as open drafts, locks and all, so nothing is lost and they can change it and submit again.
/// <para>
/// Every decision touches the submission, whose row version then makes two decisions at the same moment
/// fail rather than both miss that the other was the last one needed.
/// </para>
/// </summary>
public sealed class SubmissionDecider
{
    private readonly PuSpecSheetDbContext db;
    private readonly SheetPublisher publisher;
    private readonly ICurrentUser currentUser;
    private readonly TimeProvider clock;

    public SubmissionDecider(
        PuSpecSheetDbContext db,
        SheetPublisher publisher,
        ICurrentUser currentUser,
        TimeProvider clock)
    {
        this.db = db;
        this.publisher = publisher;
        this.currentUser = currentUser;
        this.clock = clock;
    }

    /// <returns>The submission as it now stands.</returns>
    public async Task<DbSheetSubmission> ApproveAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await LoadPendingAsync(submissionId, cancellationToken);
        var approver = MyPendingDecision(submission);
        var now = Now();
        approver.Decision = ApprovalDecision.Approved;
        approver.DecidedAtUtc = now;
        db.MarkModified(submission);

        if (submission.Approvers.All(candidate => candidate.Decision == ApprovalDecision.Approved))
        {
            submission.Status = SubmissionStatus.Approved;
            submission.DecidedAtUtc = now;
            await publisher.PublishSubmissionAsync(submission, cancellationToken);
        }
        else
        {
            await db.SaveSheetChangesAsync(cancellationToken);
        }

        return submission;
    }

    public async Task<DbSheetSubmission> RejectAsync(int submissionId, string reason, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new InvalidRequestException("Say why the changes are rejected.");
        }

        var submission = await LoadPendingAsync(submissionId, cancellationToken);
        var approver = MyPendingDecision(submission);
        var now = Now();
        approver.Decision = ApprovalDecision.Rejected;
        approver.DecidedAtUtc = now;
        approver.Reason = reason.Trim();
        await CloseAsync(submission, SubmissionStatus.Rejected, now, cancellationToken);
        return submission;
    }

    public async Task<DbSheetSubmission> WithdrawAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await LoadPendingAsync(submissionId, cancellationToken);
        if (submission.SubmittedByUserId != currentUser.UserId)
        {
            throw new ForbiddenException("Only the person who submitted these changes can withdraw them.");
        }

        await CloseAsync(submission, SubmissionStatus.Withdrawn, Now(), cancellationToken);
        return submission;
    }

    /// <summary>Ends the submission without publishing and hands its drafts back to their authors.</summary>
    private async Task CloseAsync(DbSheetSubmission submission, SubmissionStatus status, DateTime now, CancellationToken cancellationToken)
    {
        submission.Status = status;
        submission.DecidedAtUtc = now;
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        await db.SaveSheetChangesAsync(cancellationToken);
        await SubmittedDrafts.ReleaseAsync(db, submission.SheetId, submission.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<DbSheetSubmission> LoadPendingAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await SheetSubmissionQueries.FindWithApproversForEditAsync(db, submissionId, cancellationToken)
            ?? throw new NotFoundException($"Submission {submissionId} was not found.");
        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new ConflictException("These changes have already been approved, rejected or withdrawn.");
        }

        return submission;
    }

    private DbSheetSubmissionApprover MyPendingDecision(DbSheetSubmission submission)
    {
        var me = currentUser.UserId;
        var approver = submission.Approvers.SingleOrDefault(candidate => candidate.UserId == me)
            ?? throw new ForbiddenException("You weren't asked to approve these changes.");
        if (approver.Decision != ApprovalDecision.Pending)
        {
            throw new ConflictException("You have already decided on these changes.");
        }

        return approver;
    }

    private DateTime Now()
    {
        return clock.GetUtcNow().UtcDateTime;
    }
}
