using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Queries;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Mappers.Approvals;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

public sealed class SubmissionReader
{
    // Decided submissions stay listed for a while so the submitter can read a rejection's reason.
    private static readonly TimeSpan _decidedKeptFor = TimeSpan.FromDays(30);

    private readonly PuSpecSheetDbContext _db;
    private readonly TimeProvider _clock;
    private readonly ISubmissionDataService _submissionData;

    public SubmissionReader(PuSpecSheetDbContext db, TimeProvider clock, ISubmissionDataService submissionData)
    {
        _db = db;
        _clock = clock;
        _submissionData = submissionData;
    }

    public Task<IReadOnlyList<SheetSubmission>> AwaitingDecisionOfAsync(int userId, CancellationToken cancellationToken)
    {
        return ToModelsAsync(_submissionData.ListAwaitingDecisionOfAsync(userId, cancellationToken), cancellationToken);
    }

    public Task<IReadOnlyList<SheetSubmission>> SubmittedByAsync(int userId, CancellationToken cancellationToken)
    {
        var since = _clock.GetUtcNow().UtcDateTime - _decidedKeptFor;
        return ToModelsAsync(_submissionData.ListSubmittedByAsync(userId, since, cancellationToken), cancellationToken);
    }

    public async Task<SheetSubmission> GetAsync(int submissionId, CancellationToken cancellationToken)
    {
        var found = await ToModelsAsync(_submissionData.ListByIdAsync(submissionId, cancellationToken), cancellationToken);
        return found.SingleOrDefault() ?? throw new NotFoundException($"Submission {submissionId} was not found.");
    }

    // Once a submission is decided it has no changes of its own left to show.
    public async Task EnsureCanReviewAsync(int sheetId, int submissionId, int userId, CancellationToken cancellationToken)
    {
        var submission = await _submissionData.FindReviewAccessAsync(sheetId, submissionId, userId, cancellationToken)
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

    private async Task<IReadOnlyList<SheetSubmission>> ToModelsAsync(Task<List<DbSheetSubmission>> loading, CancellationToken cancellationToken)
    {
        var loaded = await loading;
        var counts = await CountChangesAsync(loaded.Select(submission => submission.Id).ToList(), cancellationToken);
        return loaded
            .Select(submission => submission.ToModel(counts.GetValueOrDefault(submission.Id) ?? new SubmissionChangeCounts
            {
                Tables = 0,
                Sections = 0,
                Rows = 0,
                ColumnBlocks = 0,
            }))
            .ToList();
    }

    private async Task<Dictionary<int, SubmissionChangeCounts>> CountChangesAsync(List<int> ids, CancellationToken cancellationToken)
    {
        if (ids.Count == 0)
        {
            return [];
        }

        var tables = await _db.SheetTableRevisions.CountBySubmissionAsync(ids, cancellationToken);
        var sections = await _db.SheetSectionRevisions.CountBySubmissionAsync(ids, cancellationToken);
        var rows = await _db.SheetRowRevisions.CountBySubmissionAsync(ids, cancellationToken);
        var blocks = await _db.SheetColumnBlockRevisions.CountBySubmissionAsync(ids, cancellationToken);
        return ids.ToDictionary(
            id => id,
            id => new SubmissionChangeCounts
            {
                Tables = tables.GetValueOrDefault(id),
                Sections = sections.GetValueOrDefault(id),
                Rows = rows.GetValueOrDefault(id),
                ColumnBlocks = blocks.GetValueOrDefault(id),
            });
    }
}
