using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Approvals;

/// <summary>
/// Publishing with approval. A publish becomes a pending submission; its chosen approvers each approve or
/// reject it, and only once all of them approve are its changes published as the sheet's next version.
/// </summary>
public interface ISubmissionsService
{
    /// <summary>The people the current user can ask to approve their changes.</summary>
    Task<IReadOnlyList<Approver>> ApproversAsync(CancellationToken cancellationToken);

    /// <summary>Submits drafts on the sheet for approval and returns the refreshed live sheet.</summary>
    Task<Sheet> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken);

    /// <summary>Submissions waiting for the current user's decision.</summary>
    Task<IReadOnlyList<SheetSubmission>> AwaitingMyDecisionAsync(CancellationToken cancellationToken);

    /// <summary>The current user's own submissions, pending or recently decided.</summary>
    Task<IReadOnlyList<SheetSubmission>> MineAsync(CancellationToken cancellationToken);

    Task<SheetSubmission> ApproveAsync(int submissionId, CancellationToken cancellationToken);

    Task<SheetSubmission> RejectAsync(int submissionId, RejectSubmissionRequest request, CancellationToken cancellationToken);

    Task<SheetSubmission> WithdrawAsync(int submissionId, CancellationToken cancellationToken);
}
