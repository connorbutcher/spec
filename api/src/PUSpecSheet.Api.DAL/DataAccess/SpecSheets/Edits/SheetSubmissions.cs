using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Types;
using PUSpecSheet.Api.Database;
using PUSpecSheet.Api.Database.Schema.Approval;
using PUSpecSheet.Api.Database.Schema.Spec;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Edits;

/// <summary>
/// Keeps a sheet's submission in step with the approval workflow on its draft's SpecSheets row. The workflow steps are
/// still what the screens act on; the submission is what the revisions keep of them, and its drafts point at it while
/// it is pending. Each method runs in the caller's transaction and saves.
/// </summary>
internal class SheetSubmissions
{
    private readonly PUSpecSheetDbContext _dbContext;

    public SheetSubmissions(PUSpecSheetDbContext dbContext)
    {
        ArgumentNullException.ThrowIfNull(dbContext, nameof(dbContext));

        _dbContext = dbContext;
    }

    public async Task SubmitAsync(int sheetId, int submittedByUserId, string? note, IEnumerable<int> approverUserIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(approverUserIds, nameof(approverUserIds));

        if (await FindPendingAsync(sheetId, cancellationToken) is not null)
        {
            throw new InvalidOperationException($"Sheet with Id {sheetId} already has a submission awaiting approval.");
        }

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
        await TagDraftsAsync(sheetId, submission.Id, cancellationToken);
    }

    public async Task RecordDecisionAsync(int sheetId, int userId, WorkflowState state, string? comment, CancellationToken cancellationToken)
    {
        var submission = await GetPendingAsync(sheetId, cancellationToken);
        var approver = submission.Approvers.FirstOrDefault(a => a.UserId == userId)
            ?? throw new KeyNotFoundException($"User with Id {userId} isn't an approver of the submission of Sheet with Id {sheetId}.");

        approver.Decision = state switch
        {
            WorkflowState.Approved => ApproverDecision.Approved,
            WorkflowState.Rejected => ApproverDecision.Rejected,
            _ => ApproverDecision.Pending
        };
        approver.DecidedDateTime = approver.Decision == ApproverDecision.Pending ? null : DateTime.UtcNow;
        approver.Reason = approver.Decision == ApproverDecision.Rejected ? comment ?? DbSheetSubmissionApprover.NoReasonGiven : comment;

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Rejects the submission, which hands its drafts back to be edited.
    /// </summary>
    public async Task RejectAsync(int sheetId, CancellationToken cancellationToken)
    {
        var submission = await GetPendingAsync(sheetId, cancellationToken);
        submission.Status = SubmissionStatus.Rejected;
        submission.DecidedDateTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await TagDraftsAsync(sheetId, null, cancellationToken);
    }

    /// <summary>
    /// Approves the submission. Its drafts are tagged before they are published, so call this first.
    /// </summary>
    public async Task<DbSheetSubmission> ApproveAsync(int sheetId, CancellationToken cancellationToken)
    {
        var submission = await GetPendingAsync(sheetId, cancellationToken);
        submission.Status = SubmissionStatus.Approved;
        submission.DecidedDateTime = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);
        await TagDraftsAsync(sheetId, submission.Id, cancellationToken);
        return submission;
    }

    public async Task SetVersionAsync(DbSheetSubmission submission, DbSheetVersion version, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(submission, nameof(submission));
        ArgumentNullException.ThrowIfNull(version, nameof(version));

        submission.SheetVersionId = version.Id;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ReassignAsync(int sheetId, IEnumerable<int> removedUserIds, IEnumerable<int> addedUserIds, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(removedUserIds, nameof(removedUserIds));
        ArgumentNullException.ThrowIfNull(addedUserIds, nameof(addedUserIds));

        var submission = await GetPendingAsync(sheetId, cancellationToken);
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

    private async Task<DbSheetSubmission> GetPendingAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await FindPendingAsync(sheetId, cancellationToken)
            ?? throw new InvalidOperationException($"Sheet with Id {sheetId} has no submission awaiting approval.");
    }

    private async Task<DbSheetSubmission?> FindPendingAsync(int sheetId, CancellationToken cancellationToken)
    {
        return await _dbContext.SheetSubmissions
            .Include(s => s.Approvers)
            .FirstOrDefaultAsync(s => s.SheetId == sheetId && s.Status == SubmissionStatus.Pending, cancellationToken);
    }

    // Every draft of the sheet belongs to its pending submission, or to none once the submission is rejected.
    private async Task TagDraftsAsync(int sheetId, int? submissionId, CancellationToken cancellationToken)
    {
        await _dbContext.SheetTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        await _dbContext.SheetTableGroupRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        await _dbContext.SheetTableRowRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        await _dbContext.SheetPropertyRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
        await _dbContext.SheetLinkedTableRevisions
            .Where(r => r.SheetId == sheetId && r.Status == RevisionStatus.Draft)
            .ExecuteUpdateAsync(setters => setters.SetProperty(r => r.SheetSubmissionId, submissionId), cancellationToken);
    }
}