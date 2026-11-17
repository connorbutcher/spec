using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;

namespace PUSpecSheet.DataAccess.Approvals;

// A publish becomes a pending submission; only once every chosen approver approves are its changes published as
// the next version.
public interface ISubmissionsService
{
    Task<IReadOnlyList<Approver>> ApproversAsync(CancellationToken cancellationToken);

    Task<Sheet> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<SheetSubmission>> AwaitingMyDecisionAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<SheetSubmission>> MineAsync(CancellationToken cancellationToken);

    Task<SheetSubmission> ApproveAsync(int submissionId, CancellationToken cancellationToken);

    Task<SheetSubmission> RejectAsync(int submissionId, RejectSubmissionRequest request, CancellationToken cancellationToken);

    Task<SheetSubmission> WithdrawAsync(int submissionId, CancellationToken cancellationToken);
}
