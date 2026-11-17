using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.DataAccess.Approvals.Queries;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Mappers.Approvals;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

/// <summary>
/// Reads submissions for the screens that show them (an approver's inbox, a submitter's own, one after a
/// decision) and says who may see a submission's changes.
/// </summary>
public sealed class SubmissionReader
{
    /// <summary>How far back a submitter's decided submissions are still listed, so a rejection's reason can be read.</summary>
    private static readonly TimeSpan DecidedKeptFor = TimeSpan.FromDays(30);

    private readonly PuSpecSheetDbContext db;
    private readonly TimeProvider clock;

    public SubmissionReader(PuSpecSheetDbContext db, TimeProvider clock)
    {
        this.db = db;
        this.clock = clock;
    }

    /// <summary>Pending submissions waiting for <paramref name="userId"/>'s decision, oldest first.</summary>
    public Task<IReadOnlyList<SheetSubmissionDto>> AwaitingDecisionOfAsync(int userId, CancellationToken cancellationToken)
    {
        return ToDtosAsync(SheetSubmissionQueries.ListAwaitingDecisionOfAsync(db, userId, cancellationToken), cancellationToken);
    }

    /// <summary>What <paramref name="userId"/> submitted that is still pending or was decided recently, newest first.</summary>
    public Task<IReadOnlyList<SheetSubmissionDto>> SubmittedByAsync(int userId, CancellationToken cancellationToken)
    {
        var since = clock.GetUtcNow().UtcDateTime - DecidedKeptFor;
        return ToDtosAsync(SheetSubmissionQueries.ListSubmittedByAsync(db, userId, since, cancellationToken), cancellationToken);
    }

    public async Task<SheetSubmissionDto> GetAsync(int submissionId, CancellationToken cancellationToken)
    {
        var found = await ToDtosAsync(SheetSubmissionQueries.ListByIdAsync(db, submissionId, cancellationToken), cancellationToken);
        return found.SingleOrDefault() ?? throw new NotFoundException($"Submission {submissionId} was not found.");
    }

    /// <summary>
    /// Refuses to show a submission's changes to anyone but its submitter and approvers, or once it is no
    /// longer pending, when it has no changes of its own left to show.
    /// </summary>
    public async Task EnsureCanReviewAsync(int sheetId, int submissionId, int userId, CancellationToken cancellationToken)
    {
        var submission = await SheetSubmissionQueries.FindReviewAccessAsync(db, sheetId, submissionId, userId, cancellationToken)
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

    private async Task<IReadOnlyList<SheetSubmissionDto>> ToDtosAsync(Task<List<DbSheetSubmission>> loading, CancellationToken cancellationToken)
    {
        var loaded = await loading;
        var counts = await CountChangesAsync(loaded.Select(submission => submission.Id).ToList(), cancellationToken);
        return loaded
            .Select(submission => submission.ToDto(counts.GetValueOrDefault(submission.Id) ?? new SubmissionChangeCountsDto(0, 0, 0, 0)))
            .ToList();
    }

    /// <summary>How many items each submission changes, from the revisions still tagged with it.</summary>
    private async Task<Dictionary<int, SubmissionChangeCountsDto>> CountChangesAsync(List<int> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var tables = await db.SheetTableRevisions.CountBySubmissionAsync(ids, cancellationToken);
        var sections = await db.SheetSectionRevisions.CountBySubmissionAsync(ids, cancellationToken);
        var rows = await db.SheetRowRevisions.CountBySubmissionAsync(ids, cancellationToken);
        var blocks = await db.SheetColumnBlockRevisions.CountBySubmissionAsync(ids, cancellationToken);
        return ids.ToDictionary(
            id => id,
            id => new SubmissionChangeCountsDto(
                tables.GetValueOrDefault(id),
                sections.GetValueOrDefault(id),
                rows.GetValueOrDefault(id),
                blocks.GetValueOrDefault(id)));
    }
}
