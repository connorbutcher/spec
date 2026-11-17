using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Publishing;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

// The last approval publishes. A single rejection, which must say why, rejects the whole submission at once.
public sealed class SubmissionDecider
{
    private readonly PuSpecSheetDbContext _db;
    private readonly SheetPublisher _publisher;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;
    private readonly ISubmissionDataService _submissionData;

    public SubmissionDecider(
        PuSpecSheetDbContext db,
        SheetPublisher publisher,
        ICurrentUser currentUser,
        TimeProvider clock,
        ISubmissionDataService submissionData)
    {
        _db = db;
        _publisher = publisher;
        _currentUser = currentUser;
        _clock = clock;
        _submissionData = submissionData;
    }

    public async Task<DbSheetSubmission> ApproveAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await LoadPendingAsync(submissionId, cancellationToken);
        var approver = MyPendingDecision(submission);
        var now = Now();
        approver.Decision = ApprovalDecision.Approved;
        approver.DecidedAtUtc = now;
        _db.MarkModified(submission);

        if (submission.Approvers.All(candidate => candidate.Decision == ApprovalDecision.Approved))
        {
            submission.Status = SubmissionStatus.Approved;
            submission.DecidedAtUtc = now;
            await _publisher.PublishSubmissionAsync(submission, cancellationToken);
        }
        else
        {
            await _db.SaveSheetChangesAsync(cancellationToken);
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
        if (submission.SubmittedByUserId != _currentUser.UserId)
        {
            throw new ForbiddenException("Only the person who submitted these changes can withdraw them.");
        }

        await CloseAsync(submission, SubmissionStatus.Withdrawn, Now(), cancellationToken);
        return submission;
    }

    private async Task CloseAsync(DbSheetSubmission submission, SubmissionStatus status, DateTime now, CancellationToken cancellationToken)
    {
        submission.Status = status;
        submission.DecidedAtUtc = now;
        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        await _db.SaveSheetChangesAsync(cancellationToken);
        await SubmittedDrafts.ReleaseAsync(_db, submission.SheetId, submission.Id, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task<DbSheetSubmission> LoadPendingAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await _submissionData.FindWithApproversForEditAsync(submissionId, cancellationToken)
            ?? throw new NotFoundException($"Submission {submissionId} was not found.");
        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new ConflictException("These changes have already been approved, rejected or withdrawn.");
        }

        return submission;
    }

    private DbSheetSubmissionApprover MyPendingDecision(DbSheetSubmission submission)
    {
        var me = _currentUser.UserId;
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
        return _clock.GetUtcNow().UtcDateTime;
    }
}
