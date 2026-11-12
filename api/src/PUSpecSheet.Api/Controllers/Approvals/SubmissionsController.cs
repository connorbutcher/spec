using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Approvals;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers.Approvals;

/// <summary>
/// Publishing with approval: who can approve, what waits for whom, and approving, rejecting or withdrawing
/// a submission. Submitting is <c>POST api/sheets/{id}/publish</c>.
/// </summary>
[ApiController]
[Tags(ApiTags.Approvals)]
public sealed class SubmissionsController(ISheetSubmissionService submissions) : ControllerBase
{
    /// <summary>List possible approvers</summary>
    /// <remarks>Everyone who can approve changes, apart from you: nobody approves their own.</remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The people you can ask, by name.</response>
    [HttpGet("api/approvers")]
    [Authorize(Policy = PermissionKeys.SheetsPublish)]
    public async Task<ActionResult<IReadOnlyList<ApproverDto>>> Approvers(CancellationToken cancellationToken)
    {
        var result = await submissions.ApproversAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>List submissions waiting for you</summary>
    /// <remarks>Pending submissions you were asked to approve and haven't decided on yet, oldest first.</remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The submissions waiting for your decision.</response>
    [HttpGet("api/submissions/awaiting-me")]
    public async Task<ActionResult<IReadOnlyList<SheetSubmissionDto>>> AwaitingMe(CancellationToken cancellationToken)
    {
        var result = await submissions.AwaitingMyDecisionAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>List your submissions</summary>
    /// <remarks>What you submitted that is still waiting, or was decided in the last 30 days, newest first.</remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Your submissions.</response>
    [HttpGet("api/submissions/mine")]
    public async Task<ActionResult<IReadOnlyList<SheetSubmissionDto>>> Mine(CancellationToken cancellationToken)
    {
        var result = await submissions.MineAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>List a sheet's pending submissions</summary>
    /// <param name="id">The sheet's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet's submissions waiting for approval, oldest first.</response>
    [HttpGet("api/sheets/{id:int}/submissions")]
    public async Task<ActionResult<IReadOnlyList<SheetSubmissionDto>>> PendingOnSheet(int id, CancellationToken cancellationToken)
    {
        var result = await submissions.PendingOnSheetAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Approve a submission</summary>
    /// <remarks>
    /// Records your approval. When you are the last of its approvers to approve, its changes are published
    /// as the sheet's next version, attributed to the submitter and with their note.
    /// </remarks>
    /// <param name="id">The submission's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The submission as it now stands.</response>
    /// <response code="403">You weren't asked to approve it.</response>
    /// <response code="404">The submission doesn't exist.</response>
    /// <response code="409">It was already decided, you already decided, or someone decided at the same moment.</response>
    [HttpPost("api/submissions/{id:int}/approve")]
    [Authorize(Policy = PermissionKeys.SheetsApprove)]
    public async Task<ActionResult<SheetSubmissionDto>> Approve(int id, CancellationToken cancellationToken)
    {
        var result = await submissions.ApproveAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Reject a submission</summary>
    /// <remarks>
    /// Rejects it, with a reason the submitter sees. One rejection is enough: nothing is published, and the
    /// changes go back to their authors as drafts they can change and submit again.
    /// </remarks>
    /// <param name="id">The submission's id.</param>
    /// <param name="request">Why it is rejected.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The rejected submission.</response>
    /// <response code="400">No reason was given.</response>
    /// <response code="403">You weren't asked to approve it.</response>
    /// <response code="404">The submission doesn't exist.</response>
    /// <response code="409">It was already decided, you already decided, or someone decided at the same moment.</response>
    [HttpPost("api/submissions/{id:int}/reject")]
    [Authorize(Policy = PermissionKeys.SheetsApprove)]
    public async Task<ActionResult<SheetSubmissionDto>> Reject(int id, RejectSubmissionRequest request, CancellationToken cancellationToken)
    {
        var result = await submissions.RejectAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Withdraw your submission</summary>
    /// <remarks>Takes back a submission of yours that is still waiting; its changes go back to being drafts.</remarks>
    /// <param name="id">The submission's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The withdrawn submission.</response>
    /// <response code="403">Someone else submitted it.</response>
    /// <response code="404">The submission doesn't exist.</response>
    /// <response code="409">It was already decided, or someone decided at the same moment.</response>
    [HttpPost("api/submissions/{id:int}/withdraw")]
    [Authorize(Policy = PermissionKeys.SheetsPublish)]
    public async Task<ActionResult<SheetSubmissionDto>> Withdraw(int id, CancellationToken cancellationToken)
    {
        var result = await submissions.WithdrawAsync(id, cancellationToken);
        return Ok(result);
    }
}
