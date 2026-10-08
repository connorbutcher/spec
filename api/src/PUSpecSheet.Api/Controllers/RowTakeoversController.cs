using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets.Collaboration;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Asking to take over a row that is checked out to someone else, and answering such a request. The
/// people involved hear about each step over the live connection as well.
/// </summary>
[ApiController]
[Tags(ApiTags.SheetRows)]
[Authorize(Policy = PermissionKeys.SheetsEdit)]
public sealed class RowTakeoversController(IRowTakeoverService takeovers) : ControllerBase
{
    /// <summary>Check whether a row can be taken over</summary>
    /// <remarks>
    /// Says whether you could ask to take over the row right now. When you couldn't, <c>reason</c> says why
    /// in words to show: the row has unpublished changes, isn't checked out, is already yours, or someone
    /// else has asked for it.
    /// </remarks>
    /// <param name="rowId">The sheet row's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Whether a takeover can be requested, and why not when it can't.</response>
    [HttpGet("api/sheet-rows/{rowId:int}/takeover-availability")]
    public async Task<ActionResult<RowTakeoverAvailabilityDto>> GetAvailability(int rowId, CancellationToken cancellationToken)
    {
        var result = await takeovers.GetAvailabilityAsync(rowId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Ask to take over a row</summary>
    /// <remarks>
    /// Asks the person a row is checked out to for it. Only a row they haven't changed can be asked for:
    /// once it has unpublished changes it stays with them until they publish or discard. If they don't
    /// have the sheet open the row is yours straight away (<c>GrantedHolderAway</c>). Otherwise the request
    /// comes back <c>Pending</c>: they can approve or deny it, and if they do neither before
    /// <c>expiresAtUtc</c> it is granted (<c>GrantedOnTimeout</c>). If they change the row while the
    /// request waits, it closes as <c>KeptForChanges</c>.
    /// </remarks>
    /// <param name="rowId">The sheet row's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The request and where it has got to.</response>
    /// <response code="400">The row is already checked out to you.</response>
    /// <response code="409">The row has unpublished changes, isn't checked out to anyone, or someone else has already asked for it.</response>
    [HttpPost("api/sheet-rows/{rowId:int}/takeover-requests")]
    public async Task<ActionResult<RowTakeoverDto>> Ask(int rowId, CancellationToken cancellationToken)
    {
        var result = await takeovers.RequestAsync(rowId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Approve a takeover request</summary>
    /// <remarks>
    /// Hands the row to the person who asked. Only the person the row is checked out to can approve. If
    /// you have changed the row since they asked, it stays with you and the request closes as
    /// <c>KeptForChanges</c>.
    /// </remarks>
    /// <param name="id">The takeover request's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The settled request.</response>
    /// <response code="404">The request has already been answered, withdrawn or granted.</response>
    [HttpPost("api/row-takeovers/{id:guid}/approve")]
    public async Task<ActionResult<RowTakeoverDto>> Approve(Guid id, CancellationToken cancellationToken)
    {
        var result = await takeovers.ApproveAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Deny a takeover request</summary>
    /// <remarks>
    /// Keeps the row checked out to you. Only the person the row is checked out to can deny.
    /// </remarks>
    /// <param name="id">The takeover request's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The settled request.</response>
    /// <response code="404">The request has already been answered, withdrawn or granted.</response>
    [HttpPost("api/row-takeovers/{id:guid}/deny")]
    public async Task<ActionResult<RowTakeoverDto>> Deny(Guid id, CancellationToken cancellationToken)
    {
        var result = await takeovers.DenyAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Withdraw a takeover request</summary>
    /// <remarks>
    /// Withdraws a request you made that hasn't been answered yet.
    /// </remarks>
    /// <param name="id">The takeover request's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The settled request.</response>
    /// <response code="404">The request has already been answered, withdrawn or granted.</response>
    [HttpDelete("api/row-takeovers/{id:guid}")]
    public async Task<ActionResult<RowTakeoverDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        var result = await takeovers.CancelAsync(id, cancellationToken);
        return Ok(result);
    }
}
