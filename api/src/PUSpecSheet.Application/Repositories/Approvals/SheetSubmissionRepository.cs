using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Persistence.Approvals;
using PUSpecSheet.Database.Entities.Approvals;
using PUSpecSheet.Domain.Approvals;

using PUSpecSheet.Database;

namespace PUSpecSheet.Application.Repositories.Approvals;

public sealed class SheetSubmissionRepository : ISheetSubmissionRepository
{
    private readonly PuSpecSheetDbContext db;

    public SheetSubmissionRepository(PuSpecSheetDbContext db)
    {
        this.db = db;
    }

    public Task<List<SheetSubmission>> ListAwaitingDecisionOfAsync(int userId, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.Status == SubmissionStatus.Pending
                    && submission.Approvers.Any(approver => approver.UserId == userId && approver.Decision == ApprovalDecision.Pending))
                .OrderBy(submission => submission.SubmittedAtUtc),
            cancellationToken);
    }

    public Task<List<SheetSubmission>> ListSubmittedByAsync(int userId, DateTime decidedSince, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.SubmittedByUserId == userId
                    && (submission.Status == SubmissionStatus.Pending || submission.DecidedAtUtc >= decidedSince))
                .OrderByDescending(submission => submission.SubmittedAtUtc)
                .Take(50),
            cancellationToken);
    }

    public Task<List<SheetSubmission>> ListPendingOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.SheetId == sheetId && submission.Status == SubmissionStatus.Pending)
                .OrderBy(submission => submission.SubmittedAtUtc),
            cancellationToken);
    }

    public Task<List<SheetSubmission>> ListByIdAsync(int id, CancellationToken cancellationToken)
    {
        return ListAsync(db.SheetSubmissions.Where(submission => submission.Id == id), cancellationToken);
    }

    public async Task<SubmissionReviewAccess?> FindReviewAccessAsync(int sheetId, int id, int userId, CancellationToken cancellationToken)
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

    public async Task<SheetSubmission?> FindWithApproversForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await db.SheetSubmissions
            .Include(candidate => candidate.Approvers)
            .SingleOrDefaultAsync(candidate => candidate.Id == id, cancellationToken);
    }

    public void Add(SheetSubmission submission)
    {
        db.SheetSubmissions.Add(submission);
    }

    private static async Task<List<SheetSubmission>> ListAsync(IQueryable<SheetSubmission> query, CancellationToken cancellationToken)
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
