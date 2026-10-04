using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Changes to one row on a sheet. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Tags(ApiTags.SheetRows)]
[Route("api/sheet-rows")]
public sealed class SheetRowsController(ISheetRowService rows) : ControllerBase
{
    /// <summary>Lock a row</summary>
    /// <remarks>
    /// Locks the row to you without changing it yet, so nobody else can edit it while you do. The lock is released when you publish or discard.
    /// </remarks>
    /// <param name="id">The sheet row's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="409">The row is locked by someone else.</response>
    [HttpPost("{id:int}/lock")]
    public async Task<ActionResult<SheetDto>> Lock(int id, CancellationToken cancellationToken)
    {
        var result = await rows.LockAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Save a row's values</summary>
    /// <remarks>
    /// Sets or clears cell values in the row, which locks it to you until you publish or discard. For each
    /// cell, set the field that matches its kind: <c>text</c>, <c>number</c>, <c>date</c>, <c>boolean</c>, or
    /// <c>optionId</c> for a dropdown. Leaving them all unset clears the cell. Cells not listed are unchanged.
    /// </remarks>
    /// <param name="id">The sheet row's id.</param>
    /// <param name="request">The cells to change and their new values.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="400">A value doesn't fit its cell: the wrong kind, out of range, or not one of the dropdown's options.</response>
    /// <response code="409">The row is locked by someone else.</response>
    [HttpPut("{id:int}/values")]
    public async Task<ActionResult<SheetDto>> SaveValues(int id, SaveRowValuesRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.SaveValuesAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Move a row</summary>
    /// <remarks>
    /// Moves the row to a position among its section's rows.
    /// </remarks>
    /// <param name="id">The sheet row's id.</param>
    /// <param name="request">The 1-based position to move to. A position past the end moves it last.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<SheetDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Remove a row</summary>
    /// <remarks>
    /// Removes the row. A row nobody has published is simply deleted; otherwise the removal is your draft until you publish. Not allowed in the header.
    /// </remarks>
    /// <param name="id">The sheet row's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<SheetDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await rows.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Discard a row's draft</summary>
    /// <remarks>
    /// Throws away your draft on the row, putting back what was last published and releasing its lock.
    /// </remarks>
    /// <param name="id">The sheet row's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpDelete("{id:int}/draft")]
    public async Task<ActionResult<SheetDto>> Discard(int id, CancellationToken cancellationToken)
    {
        var result = await rows.DiscardAsync(id, cancellationToken);
        return Ok(result);
    }
}
