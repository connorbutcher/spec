using Microsoft.EntityFrameworkCore;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Data;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Application.Approvals;

/// <summary>Reads submissions for the screens that list them: an approver's inbox, a submitter's own, a sheet's pending ones.</summary>
public sealed class SubmissionQueries(PuSpecSheetDbContext db, TimeProvider clock)
{
    /// <summary>How far back a submitter's decided submissions are still listed, so a rejection's reason can be read.</summary>
    private static readonly TimeSpan DecidedKeptFor = TimeSpan.FromDays(30);

    /// <summary>Pending submissions waiting for <paramref name="userId"/>'s decision, oldest first.</summary>
    public Task<IReadOnlyList<SheetSubmissionDto>> AwaitingDecisionOfAsync(int userId, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.Status == SubmissionStatus.Pending
                    && submission.Approvers.Any(approver => approver.UserId == userId && approver.Decision == ApprovalDecision.Pending))
                .OrderBy(submission => submission.SubmittedAtUtc),
            cancellationToken);
    }

    /// <summary>What <paramref name="userId"/> submitted that is still pending or was decided recently, newest first.</summary>
    public Task<IReadOnlyList<SheetSubmissionDto>> SubmittedByAsync(int userId, CancellationToken cancellationToken)
    {
        var since = clock.GetUtcNow().UtcDateTime - DecidedKeptFor;
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.SubmittedByUserId == userId
                    && (submission.Status == SubmissionStatus.Pending || submission.DecidedAtUtc >= since))
                .OrderByDescending(submission => submission.SubmittedAtUtc)
                .Take(50),
            cancellationToken);
    }

    /// <summary>The sheet's submissions waiting for approval, oldest first.</summary>
    public Task<IReadOnlyList<SheetSubmissionDto>> PendingOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return ListAsync(
            db.SheetSubmissions
                .Where(submission => submission.SheetId == sheetId && submission.Status == SubmissionStatus.Pending)
                .OrderBy(submission => submission.SubmittedAtUtc),
            cancellationToken);
    }

    public async Task<SheetSubmissionDto> GetAsync(int submissionId, CancellationToken cancellationToken)
    {
        var found = await ListAsync(db.SheetSubmissions.Where(submission => submission.Id == submissionId), cancellationToken);
        return found.SingleOrDefault() ?? throw new NotFoundException($"Submission {submissionId} was not found.");
    }

    /// <summary>
    /// Refuses to show a submission's changes to anyone but its submitter and approvers, or once it is no
    /// longer pending, when it has no changes of its own left to show.
    /// </summary>
    public async Task EnsureCanReviewAsync(int sheetId, int submissionId, int userId, CancellationToken cancellationToken)
    {
        var submission = await db.SheetSubmissions
            .AsNoTracking()
            .Where(candidate => candidate.Id == submissionId && candidate.SheetId == sheetId)
            .Select(candidate => new
            {
                candidate.Status,
                candidate.SubmittedByUserId,
                IsApprover = candidate.Approvers.Any(approver => approver.UserId == userId),
            })
            .SingleOrDefaultAsync(cancellationToken)
            ?? throw new NotFoundException($"Submission {submissionId} was not found on this sheet.");
        if (submission.SubmittedByUserId != userId && !submission.IsApprover)
        {
            throw new ForbiddenException("Only the submitter and the approvers can see changes waiting for approval.");
        }

        if (submission.Status != SubmissionStatus.Pending)
        {
            throw new ConflictException("These changes have already been approved, rejected or withdrawn.");
        }
    }

    private async Task<IReadOnlyList<SheetSubmissionDto>> ListAsync(IQueryable<SheetSubmission> query, CancellationToken cancellationToken)
    {
        var submissions = await query
            .AsNoTracking()
            .AsSplitQuery()
            .Include(submission => submission.Sheet).ThenInclude(sheet => sheet.Phase)
            .Include(submission => submission.Sheet).ThenInclude(sheet => sheet.SheetType)
            .Include(submission => submission.SubmittedBy)
            .Include(submission => submission.SheetVersion)
            .Include(submission => submission.Approvers).ThenInclude(approver => approver.User)
            .ToListAsync(cancellationToken);
        var counts = await CountChangesAsync(submissions.Select(submission => submission.Id).ToList(), cancellationToken);
        return submissions
            .Select(submission => ToDto(submission, counts.GetValueOrDefault(submission.Id) ?? new SubmissionChangeCountsDto(0, 0, 0, 0)))
            .ToList();
    }

    /// <summary>How many items each submission changes, from the revisions still tagged with it.</summary>
    private async Task<Dictionary<int, SubmissionChangeCountsDto>> CountChangesAsync(List<int> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var tables = await CountAsync(db.SheetTableRevisions.Select(revision => revision.SheetSubmissionId), ids, cancellationToken);
        var sections = await CountAsync(db.SheetSectionRevisions.Select(revision => revision.SheetSubmissionId), ids, cancellationToken);
        var rows = await CountAsync(db.SheetRowRevisions.Select(revision => revision.SheetSubmissionId), ids, cancellationToken);
        var blocks = await CountAsync(db.SheetColumnBlockRevisions.Select(revision => revision.SheetSubmissionId), ids, cancellationToken);
        return ids.ToDictionary(
            id => id,
            id => new SubmissionChangeCountsDto(
                tables.GetValueOrDefault(id),
                sections.GetValueOrDefault(id),
                rows.GetValueOrDefault(id),
                blocks.GetValueOrDefault(id)));
    }

    private static async Task<Dictionary<int, int>> CountAsync(IQueryable<int?> submissionIds, List<int> ids, CancellationToken cancellationToken)
    {
        return await submissionIds
            .Where(id => id != null && ids.Contains(id.Value))
            .GroupBy(id => id!.Value)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(entry => entry.Key, entry => entry.Count, cancellationToken);
    }

    private static SheetSubmissionDto ToDto(SheetSubmission submission, SubmissionChangeCountsDto changes)
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
            Utc(submission.SubmittedAtUtc),
            submission.Status,
            submission.DecidedAtUtc is { } decided ? Utc(decided) : null,
            submission.SheetVersion?.VersionNumber,
            changes,
            submission.Approvers
                .OrderBy(approver => approver.User.DisplayName, StringComparer.OrdinalIgnoreCase)
                .Select(approver => new SubmissionApproverDto(
                    approver.UserId,
                    approver.User.DisplayName,
                    approver.Decision,
                    approver.DecidedAtUtc is { } at ? Utc(at) : null,
                    approver.Reason))
                .ToList());
    }

    private static DateTime Utc(DateTime value)
    {
        return DateTime.SpecifyKind(value, DateTimeKind.Utc);
    }
}
