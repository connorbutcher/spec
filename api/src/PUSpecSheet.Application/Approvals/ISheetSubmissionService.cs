using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Application.Approvals;

/// <summary>
/// Publishing with approval. A publish becomes a pending submission; its chosen approvers each approve or
/// reject it, and only once all of them approve are its changes published as the sheet's next version.
/// </summary>
public interface ISheetSubmissionService
{
    /// <summary>The people the current user can ask to approve their changes.</summary>
    Task<IReadOnlyList<ApproverDto>> ApproversAsync(CancellationToken cancellationToken);

    /// <summary>Submits drafts on the sheet for approval and returns the refreshed live sheet.</summary>
    Task<SheetDto> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken);

    /// <summary>Submissions waiting for the current user's decision.</summary>
    Task<IReadOnlyList<SheetSubmissionDto>> AwaitingMyDecisionAsync(CancellationToken cancellationToken);

    /// <summary>The current user's own submissions, pending or recently decided.</summary>
    Task<IReadOnlyList<SheetSubmissionDto>> MineAsync(CancellationToken cancellationToken);

    /// <summary>The sheet's submissions waiting for approval.</summary>
    Task<IReadOnlyList<SheetSubmissionDto>> PendingOnSheetAsync(int sheetId, CancellationToken cancellationToken);

    Task<SheetSubmissionDto> ApproveAsync(int submissionId, CancellationToken cancellationToken);

    Task<SheetSubmissionDto> RejectAsync(int submissionId, RejectSubmissionRequest request, CancellationToken cancellationToken);

    Task<SheetSubmissionDto> WithdrawAsync(int submissionId, CancellationToken cancellationToken);
}
