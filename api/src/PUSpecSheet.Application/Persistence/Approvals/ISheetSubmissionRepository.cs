using PUSpecSheet.Database.Entities.Approvals;

namespace PUSpecSheet.Application.Persistence.Approvals;

/// <summary>
/// Publishes waiting for approval. Lists come read only with what a submission shows: its sheet's phase and
/// sheet type, who submitted it, the version it became and each approver.
/// </summary>
public interface ISheetSubmissionRepository
{
    /// <summary>The pending submissions waiting for the user's decision, oldest first.</summary>
    Task<List<SheetSubmission>> ListAwaitingDecisionOfAsync(int userId, CancellationToken cancellationToken);

    /// <summary>The user's submissions that are pending or were decided since the moment, newest first, at most 50.</summary>
    Task<List<SheetSubmission>> ListSubmittedByAsync(int userId, DateTime decidedSince, CancellationToken cancellationToken);

    /// <summary>The sheet's pending submissions, oldest first.</summary>
    Task<List<SheetSubmission>> ListPendingOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    /// <summary>The submission with everything a list shows, read only; empty when there is no such submission.</summary>
    Task<List<SheetSubmission>> ListByIdAsync(int id, CancellationToken cancellationToken);

    /// <summary>Whether the user may review the submission on the sheet; null when the sheet has no such submission.</summary>
    Task<SubmissionReviewAccess?> FindReviewAccessAsync(int sheetId, int id, int userId, CancellationToken cancellationToken);

    /// <summary>The submission with its approvers, tracked for deciding; null when there is no such submission.</summary>
    Task<SheetSubmission?> FindWithApproversForEditAsync(int id, CancellationToken cancellationToken);

    void Add(SheetSubmission submission);
}
