using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.DataAccess.Common;
using PUSpecSheet.DataAccess.Sheets.Publishing;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

/// <summary>
/// Submits drafts for publishing. The drafts a publish would take are checked as a publish checks them,
/// then tagged with a new pending submission: they stay drafts, so no published view shows them, and stay
/// locked to their authors, who can't change them until the submission is decided.
/// </summary>
public sealed class SheetSubmitter
{
    private readonly PuSpecSheetDbContext _db;
    private readonly SheetPublisher _publisher;
    private readonly ApproverDirectory _approvers;
    private readonly ICurrentUser _currentUser;
    private readonly TimeProvider _clock;

    public SheetSubmitter(
        PuSpecSheetDbContext db,
        SheetPublisher publisher,
        ApproverDirectory approvers,
        ICurrentUser currentUser,
        TimeProvider clock)
    {
        _db = db;
        _publisher = publisher;
        _approvers = approvers;
        _currentUser = currentUser;
        _clock = clock;
    }

    /// <returns>The new submission's id.</returns>
    public async Task<int> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        var me = _currentUser.UserId;
        var approverIds = await _approvers.EnsureEligibleAsync(request.ApproverUserIds, me, cancellationToken);
        var drafts = await _publisher.PrepareAsync(sheetId, request.Scope, cancellationToken);

        var submission = new DbSheetSubmission
        {
            SheetId = sheetId,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            SubmittedByUserId = me,
            SubmittedAtUtc = _clock.GetUtcNow().UtcDateTime,
            Status = SubmissionStatus.Pending,
        };
        foreach (var approverId in approverIds)
        {
            submission.Approvers.Add(new DbSheetSubmissionApprover { UserId = approverId, Decision = ApprovalDecision.Pending });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        _db.SheetSubmissions.Add(submission);
        await _db.SaveSheetChangesAsync(cancellationToken);

        // The drafts' row versions make this fail, as a conflict, if an author saves into one at the same moment.
        foreach (var draft in drafts.All)
        {
            draft.SheetSubmissionId = submission.Id;
        }

        await _db.SaveSheetChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return submission.Id;
    }
}
