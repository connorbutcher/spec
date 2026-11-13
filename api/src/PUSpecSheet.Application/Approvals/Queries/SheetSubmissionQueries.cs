using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Approvals;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Application.Approvals.Queries;

/// <summary>
/// Publishes waiting for approval. Lists come read only with what a submission shows: its sheet's phase and
/// sheet type, who submitted it, the version it became and each approver.
/// </summary>
public static class SheetSubmissionQueries
{
    /// <summary>The pending submissions waiting for the user's decision, oldest first.</summary>
    public static Task<List<DbSheetSubmission>> ListAwaitingDecisionOfAsync(PuSpecSheetDbContext db, int userId, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.Status == SubmissionStatus.Pending
                    && submission.Approvers.Any(approver => approver.UserId == userId && approver.Decision == ApprovalDecision.Pending))
                .OrderBy(submission => submission.SubmittedAtUtc),
            cancellationToken);
    }

    /// <summary>The user's submissions that are pending or were decided since the moment, newest first, at most 50.</summary>
    public static Task<List<DbSheetSubmission>> ListSubmittedByAsync(PuSpecSheetDbContext db, int userId, DateTime decidedSince, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.SubmittedByUserId == userId
                    && (submission.Status == SubmissionStatus.Pending || submission.DecidedAtUtc >= decidedSince))
                .OrderByDescending(submission => submission.SubmittedAtUtc)
                .Take(50),
            cancellationToken);
    }

    /// <summary>The sheet's pending submissions, oldest first.</summary>
    public static Task<List<DbSheetSubmission>> ListPendingOnSheetAsync(PuSpecSheetDbContext db, int sheetId, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.SheetId == sheetId && submission.Status == SubmissionStatus.Pending)
                .OrderBy(submission => submission.SubmittedAtUtc),
            cancellationToken);
    }

    /// <summary>The submission with everything a list shows, read only; empty when there is no such submission.</summary>
    public static Task<List<DbSheetSubmission>> ListByIdAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return ListAsync(db.SheetSubmissions.Where(submission => submission.Id == id), cancellationToken);
    }

    /// <summary>Whether the user may review the submission on the sheet; null when the sheet has no such submission.</summary>
    public static async Task<SubmissionReviewAccess?> FindReviewAccessAsync(PuSpecSheetDbContext db, int sheetId, int id, int userId, CancellationToken cancellationToken)
    {
        return await db.SheetSubmissions
            .AsNoTracking()
            .Where(candidate => candidate.Id == id && candidate.SheetId == sheetId)
            .Select(candidate => new SubmissionReviewAccess(
                candidate.Status,
                candidate.SubmittedByUserId,
                candidate.Approvers.Any(approver => approver.UserId == userId)))
            .SingleOrDefaultAsync(cancellationToken);
    }

    /// <summary>The submission with its approvers, tracked for deciding; null when there is no such submission.</summary>
    public static async Task<DbSheetSubmission?> FindWithApproversForEditAsync(PuSpecSheetDbContext db, int id, CancellationToken cancellationToken)
    {
        return await db.SheetSubmissions
            .Include(candidate => candidate.Approvers)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    private static async Task<List<DbSheetSubmission>> ListAsync(IQueryable<DbSheetSubmission> query, CancellationToken cancellationToken)
    {
        return await query
            .AsNoTracking()
            .AsSplitQuery()
            .Include(submission => submission.Sheet).ThenInclude(sheet => sheet.Phase)
            .Include(submission => submission.Sheet).ThenInclude(sheet => sheet.SheetType)
            .Include(submission => submission.SubmittedBy)
            .Include(submission => submission.SheetVersion)
            .Include(submission => submission.Approvers).ThenInclude(approver => approver.User)
            .ToListAsync(cancellationToken);
    }
}
