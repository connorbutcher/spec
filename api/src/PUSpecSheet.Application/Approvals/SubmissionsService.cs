using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Contracts.Sheets.Collaboration;
using PUSpecSheet.Database.Entities.Approvals;
using PUSpecSheet.Domain.Approvals;

namespace PUSpecSheet.Application.Approvals;

public sealed class SubmissionsService : ISubmissionsService
{
    private readonly ApproverDirectory approvers;
    private readonly SheetSubmitter submitter;
    private readonly SubmissionDecider decider;
    private readonly SubmissionQueries queries;
    private readonly SheetReader reader;
    private readonly ISheetLiveNotifier notifier;
    private readonly ICurrentUser currentUser;

    public SubmissionsService(
        ApproverDirectory approvers,
        SheetSubmitter submitter,
        SubmissionDecider decider,
        SubmissionQueries queries,
        SheetReader reader,
        ISheetLiveNotifier notifier,
        ICurrentUser currentUser)
    {
        this.approvers = approvers;
        this.submitter = submitter;
        this.decider = decider;
        this.queries = queries;
        this.reader = reader;
        this.notifier = notifier;
        this.currentUser = currentUser;
    }

    public Task<IReadOnlyList<ApproverDto>> ApproversAsync(CancellationToken cancellationToken)
    {
        return approvers.ListAsync(currentUser.UserId, cancellationToken);
    }

    public async Task<SheetDto> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        await submitter.SubmitAsync(sheetId, request, cancellationToken);
        return await reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public Task<IReadOnlyList<SheetSubmissionDto>> AwaitingMyDecisionAsync(CancellationToken cancellationToken)
    {
        return queries.AwaitingDecisionOfAsync(currentUser.UserId, cancellationToken);
    }

    public Task<IReadOnlyList<SheetSubmissionDto>> MineAsync(CancellationToken cancellationToken)
    {
        return queries.SubmittedByAsync(currentUser.UserId, cancellationToken);
    }

    public Task<IReadOnlyList<SheetSubmissionDto>> PendingOnSheetAsync(int sheetId, CancellationToken cancellationToken)
    {
        return queries.PendingOnSheetAsync(sheetId, cancellationToken);
    }

    public async Task<SheetSubmissionDto> ApproveAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await decider.ApproveAsync(submissionId, cancellationToken);
        return await AfterDecisionAsync(submission, cancellationToken);
    }

    public async Task<SheetSubmissionDto> RejectAsync(int submissionId, RejectSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await decider.RejectAsync(submissionId, request.Reason, cancellationToken);
        return await AfterDecisionAsync(submission, cancellationToken);
    }

    public async Task<SheetSubmissionDto> WithdrawAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await decider.WithdrawAsync(submissionId, cancellationToken);
        return await AfterDecisionAsync(submission, cancellationToken);
    }

    /// <summary>
    /// Once a submission is decided, its locks have changed (released to their authors, or published), so
    /// everyone with the sheet open reads it again. A single approval among several changes nothing they see.
    /// </summary>
    private async Task<SheetSubmissionDto> AfterDecisionAsync(SheetSubmission submission, CancellationToken cancellationToken)
    {
        if (submission.Status != SubmissionStatus.Pending)
        {
            await notifier.SheetChangedAsync(submission.SheetId, SheetChangedNoticeDto.Everything, exceptConnectionId: null, cancellationToken);
        }

        return await queries.GetAsync(submission.Id, cancellationToken);
    }
}
