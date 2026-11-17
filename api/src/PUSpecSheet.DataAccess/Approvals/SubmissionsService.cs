using PUSpecSheet.Abstractions.Approvals;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Sheets.Collaboration;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Users;
using PUSpecSheet.Database.Models.Approvals;

namespace PUSpecSheet.DataAccess.Approvals;

public sealed class SubmissionsService : ISubmissionsService
{
    private readonly ApproverDirectory _approvers;
    private readonly SheetSubmitter _submitter;
    private readonly SubmissionDecider _decider;
    private readonly SubmissionReader _submissionReader;
    private readonly SheetReader _reader;
    private readonly ISheetLiveNotifier _notifier;
    private readonly ICurrentUser _currentUser;

    public SubmissionsService(
        ApproverDirectory approvers,
        SheetSubmitter submitter,
        SubmissionDecider decider,
        SubmissionReader submissionReader,
        SheetReader reader,
        ISheetLiveNotifier notifier,
        ICurrentUser currentUser)
    {
        _approvers = approvers;
        _submitter = submitter;
        _decider = decider;
        _submissionReader = submissionReader;
        _reader = reader;
        _notifier = notifier;
        _currentUser = currentUser;
    }

    public Task<IReadOnlyList<ApproverDto>> ApproversAsync(CancellationToken cancellationToken)
    {
        return _approvers.ListAsync(_currentUser.UserId, cancellationToken);
    }

    public async Task<SheetDto> SubmitAsync(int sheetId, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        await _submitter.SubmitAsync(sheetId, request, cancellationToken);
        return await _reader.ReadLiveAsync(sheetId, cancellationToken);
    }

    public Task<IReadOnlyList<SheetSubmissionDto>> AwaitingMyDecisionAsync(CancellationToken cancellationToken)
    {
        return _submissionReader.AwaitingDecisionOfAsync(_currentUser.UserId, cancellationToken);
    }

    public Task<IReadOnlyList<SheetSubmissionDto>> MineAsync(CancellationToken cancellationToken)
    {
        return _submissionReader.SubmittedByAsync(_currentUser.UserId, cancellationToken);
    }

    public async Task<SheetSubmissionDto> ApproveAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await _decider.ApproveAsync(submissionId, cancellationToken);
        return await AfterDecisionAsync(submission, cancellationToken);
    }

    public async Task<SheetSubmissionDto> RejectAsync(int submissionId, RejectSubmissionRequest request, CancellationToken cancellationToken)
    {
        var submission = await _decider.RejectAsync(submissionId, request.Reason, cancellationToken);
        return await AfterDecisionAsync(submission, cancellationToken);
    }

    public async Task<SheetSubmissionDto> WithdrawAsync(int submissionId, CancellationToken cancellationToken)
    {
        var submission = await _decider.WithdrawAsync(submissionId, cancellationToken);
        return await AfterDecisionAsync(submission, cancellationToken);
    }

    /// <summary>
    /// Once a submission is decided, its locks have changed (released to their authors, or published), so
    /// everyone with the sheet open reads it again. A single approval among several changes nothing they see.
    /// </summary>
    private async Task<SheetSubmissionDto> AfterDecisionAsync(DbSheetSubmission submission, CancellationToken cancellationToken)
    {
        if (submission.Status != SubmissionStatus.Pending)
        {
            await _notifier.SheetChangedAsync(submission.SheetId, SheetChangedNoticeDto.Everything, exceptConnectionId: null, cancellationToken);
        }

        return await _submissionReader.GetAsync(submission.Id, cancellationToken);
    }
}
