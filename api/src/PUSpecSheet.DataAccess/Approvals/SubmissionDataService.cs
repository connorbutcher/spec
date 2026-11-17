using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

public sealed class SubmissionDataService : ISubmissionDataService
{
    private readonly PuSpecSheetDbContext _db;

    public SubmissionDataService(PuSpecSheetDbContext db)
    {
        _db = db;
    }

    public Task<List<DbSheetSubmission>> ListAwaitingDecisionOfAsync(int userId, CancellationToken cancellationToken)
    {
        return ListAsync(
            _db.SheetSubmissions
                .Where(submission => submission.Status == SubmissionStatus.Pending
                    && submission.Approvers.Any(approver => approver.UserId == userId && approver.Decision == ApprovalDecision.Pending))
                .OrderBy(submission => submission.SubmittedAtUtc),
            cancellationToken);
    }

    public Task<List<DbSheetSubmission>> ListSubmittedByAsync(int userId, DateTime decidedSince, CancellationToken cancellationToken)
    {
        return ListAsync(
            _db.SheetSubmissions
                .Where(submission => submission.SubmittedByUserId == userId
                    && (submission.Status == SubmissionStatus.Pending || submission.DecidedAtUtc >= decidedSince))
                .OrderByDescending(submission => submission.SubmittedAtUtc)
                .Take(50),
            cancellationToken);
    }

    public Task<List<DbSheetSubmission>> ListByIdAsync(int id, CancellationToken cancellationToken)
    {
        return ListAsync(_db.SheetSubmissions.Where(submission => submission.Id == id), cancellationToken);
    }

    public async Task<int?> FindSheetIdAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetSubmissions
            .Where(candidate => candidate.Id == id)
            .Select(candidate => (int?)candidate.SheetId)
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<SubmissionReviewAccess?> FindReviewAccessAsync(int sheetId, int id, int userId, CancellationToken cancellationToken)
    {
        return await _db.SheetSubmissions
            .AsNoTracking()
            .Where(candidate => candidate.Id == id && candidate.SheetId == sheetId)
            .Select(candidate => new SubmissionReviewAccess
            {
                Status = candidate.Status,
                SubmittedByUserId = candidate.SubmittedByUserId,
                IsApprover = candidate.Approvers.Any(approver => approver.UserId == userId),
            })
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<DbSheetSubmission?> FindWithApproversForEditAsync(int id, CancellationToken cancellationToken)
    {
        return await _db.SheetSubmissions
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
