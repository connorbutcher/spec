using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// A user's checked-out changes to a sheet, submitted together. Submitting tags the user's open drafts with the
/// submission, which freezes them for everyone until it is decided: approval publishes them, rejection or withdrawal hands
/// them back, still checked out to the user. Several users can each have a submission pending on one sheet. Each method
/// runs in the caller's transaction and saves.
/// </summary>
internal class SheetSubmissions
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SheetSubmissions(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    /// <summary>
    /// Submits the user's open drafts on the sheet. Throws when they have none.
    /// </summary>
    public async Task<DbSheetSubmission> SubmitAsync(int sheetId, int submittedByUserId, string? note, IEnumerable<int> approverUserIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(approverUserIds, nameof(approverUserIds));

        var submission = new DbSheetSubmission
        {
            Id = 0,
            SheetId = sheetId,
            Note = note,
            SubmittedByUserId = submittedByUserId,
            SubmittedDateTime = DateTime.UtcNow,
            Status = SubmissionStatus.Pending,
            DecidedDateTime = null,
            SheetVersionId = null,
            Approvers = [.. approverUserIds.Distinct().Select(Approver)]
        };

        await _dbContext.SheetSubmissions.AddAsync(submission, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        var tagged = await TagOpenDraftsAsync(sheetId, submittedByUserId, submission.Id, cancellationToken);
        if (tagged == 0)
        {
            throw new InvalidOperationException("You have no changes checked out on this sheet to submit.");
        }

        return submission;
    }

    public async Task RecordDecisionAsync(DbSheetSubmission submission, int userId, ApproverDecision decision, string? comment, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submission, nameof(submission));

        var approver = submission.Approvers.FirstOrDefault(a => a.UserId == userId)
            ?? throw new KeyNotFoundException($"User with Id {userId} isn't an approver of submission {submission.Id}.");

        approver.Decision = decision;
        approver.DecidedDateTime = decision == ApproverDecision.Pending ? null : DateTime.UtcNow;
        approver.Reason = decision == ApproverDecision.Rejected ? comment ?? DbSheetSubmissionApprover.NoReasonGiven : comment;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Ends the submission without publishing it (rejected or withdrawn), which hands its drafts back to their author.
    /// </summary>
    public async Task HandBackAsync(DbSheetSubmission submission, SubmissionStatus status, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submission, nameof(submission));

        submission.Status = status;
        submission.DecidedDateTime = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);

        await UntagDraftsAsync(submission.Id, cancellationToken);
    }

    /// <summary>
    /// Marks the submission approved. Publish its drafts afterwards, then name the version with <see cref="SetVersionAsync"/>.
    /// </summary>
    public async Task ApproveAsync(DbSheetSubmission submission, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submission, nameof(submission));

        submission.Status = SubmissionStatus.Approved;
        submission.DecidedDateTime = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task SetVersionAsync(DbSheetSubmission submission, DbSheetVersion version, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submission, nameof(submission));
        ArgumentNullException.ThrowIfNull(version, nameof(version));

        submission.SheetVersionId = version.Id;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReassignAsync(DbSheetSubmission submission, IEnumerable<int> removedUserIds, IEnumerable<int> addedUserIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submission, nameof(submission));
        ArgumentNullException.ThrowIfNull(removedUserIds, nameof(removedUserIds));
        ArgumentNullException.ThrowIfNull(addedUserIds, nameof(addedUserIds));

        foreach (var approver in submission.Approvers.Where(a => removedUserIds.Contains(a.UserId)).ToList())
        {
            submission.Approvers.Remove(approver);
            _dbContext.SheetSubmissionApprovers.Remove(approver);
        }

        foreach (var userId in addedUserIds.Where(id => submission.Approvers.All(a => a.UserId != id)).Distinct())
        {
            submission.Approvers.Add(Approver(userId));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// The sheet's submission with this id that is awaiting approval, with its approvers, tracked.
    /// </summary>
    public async Task<DbSheetSubmission> GetPendingAsync(int sheetId, int submissionId, CancellationToken cancellationToken)
    {
        var submission = await _dbContext.SheetSubmissions
            .Include(s => s.Approvers)
            .FirstOrDefaultAsync(s => s.Id == submissionId && s.SheetId == sheetId, cancellationToken)
            ?? throw new KeyNotFoundException($"Submission {submissionId} not found on Sheet with Id {sheetId}.");
        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new InvalidOperationException("This submission has already been decided.");
        }

        return submission;
    }

    private static DbSheetSubmissionApprover Approver(int userId)
    {
        return new DbSheetSubmissionApprover
        {
            SheetSubmissionId = 0,
            UserId = userId,
            Decision = ApproverDecision.Pending,
            DecidedDateTime = null,
            Reason = null
        };
    }

    private async Task<int> TagOpenDraftsAsync(int sheetId, int userId, int submissionId, CancellationToken cancellationToken)
    {
        var tagged = await _dbContext.SheetTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        tagged += await _dbContext.SheetTableGroupRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        tagged += await _dbContext.SheetTableRowRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        tagged += await _dbContext.SheetPropertyRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        tagged += await _dbContext.SheetLinkedTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft && r.CreatedByUserId == userId && r.SheetSubmissionId == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        return tagged;
    }

    private async Task UntagDraftsAsync(int submissionId, CancellationToken cancellationToken)
    {
        await _dbContext.SheetTableRevisions
            .Where(r => r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, (int?)null), cancellationToken);
        await _dbContext.SheetTableGroupRevisions
            .Where(r => r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, (int?)null), cancellationToken);
        await _dbContext.SheetTableRowRevisions
            .Where(r => r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, (int?)null), cancellationToken);
        await _dbContext.SheetPropertyRevisions
            .Where(r => r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, (int?)null), cancellationToken);
        await _dbContext.SheetLinkedTableRevisions
            .Where(r => r.Status == RevisionStatus.Draft && r.SheetSubmissionId == submissionId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, (int?)null), cancellationToken);
    }
}
