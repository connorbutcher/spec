using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Opening, publishing and discarding sheets. Every action returns the sheet as the current user sees it.
/// A sheet is shown live, as of a version number (<c>version</c>) or as of a date and time (<c>asOf</c>).
/// </summary>
[ApiController]
[Tags(ApiTags.Sheets)]
public sealed class SheetsController(ISheetService sheets, ISheetTableService tables) : ControllerBase
{
    /// <summary>Open a phase's sheet</summary>
    /// <remarks>
    /// Opens the sheet of a sheet type for a phase, creating it empty the first time. With neither
    /// <c>version</c> nor <c>asOf</c> the view is live: the latest published state with your own drafts on top
    /// and other people's locks marked. A past view is read-only.
    /// </remarks>
    /// <param name="phaseId">The phase's id.</param>
    /// <param name="sheetTypeId">The sheet type's id.</param>
    /// <param name="version" example="3">Show the sheet as of this version number. Leave out for the live view.</param>
    /// <param name="asOf" example="2026-09-30T14:02:11Z">Show the sheet as it was published at this date and time (UTC). Can't be combined with <c>version</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet as you see it.</response>
    /// <response code="404">The phase or version doesn't exist, or the sheet type isn't available to this phase.</response>
    [HttpGet("api/phases/{phaseId:int}/sheets/{sheetTypeId:int}")]
    public async Task<ActionResult<SheetDto>> Open(
        int phaseId,
        int sheetTypeId,
        [FromQuery] int? version,
        [FromQuery] DateTime? asOf,
        CancellationToken cancellationToken)
    {
        var result = await sheets.OpenAsync(phaseId, sheetTypeId, SheetViewPoint.From(version, asOf), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a sheet</summary>
    /// <remarks>
    /// The sheet with this id, live or as of a version number or a date and time.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="version" example="3">Show the sheet as of this version number. Leave out for the live view.</param>
    /// <param name="asOf" example="2026-09-30T14:02:11Z">Show the sheet as it was published at this date and time (UTC). Can't be combined with <c>version</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet as you see it.</response>
    /// <response code="404">The sheet or version doesn't exist.</response>
    [HttpGet("api/sheets/{id:int}")]
    public async Task<ActionResult<SheetDto>> Get(
        int id,
        [FromQuery] int? version,
        [FromQuery] DateTime? asOf,
        CancellationToken cancellationToken)
    {
        var result = await sheets.GetAsync(id, SheetViewPoint.From(version, asOf), cancellationToken);
        return Ok(result);
    }

    /// <summary>Add a table to a sheet</summary>
    /// <remarks>
    /// Adds a table built from the latest version of a table template of the sheet's type, with its header and starting sections. The table is your draft until you publish.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="request">The table template to build the table from.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpPost("api/sheets/{id:int}/tables")]
    [Authorize(Policy = PermissionKeys.SheetsEdit)]
    public async Task<ActionResult<SheetDto>> AddTable(int id, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        var result = await tables.AddAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Publish your drafts</summary>
    /// <remarks>
    /// Publishes all of your drafts on the sheet as its next version, with an optional note, and releases your locks. Other people's drafts are untouched.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="request">An optional note describing the version.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="400">You have nothing to publish, or required cells are still empty.</response>
    [HttpPost("api/sheets/{id:int}/publish")]
    [Authorize(Policy = PermissionKeys.SheetsPublish)]
    public async Task<ActionResult<SheetDto>> Publish(int id, PublishSheetRequest request, CancellationToken cancellationToken)
    {
        var result = await sheets.PublishAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Discard your drafts</summary>
    /// <remarks>
    /// Throws away all of your drafts on the sheet and releases your locks. Other people's drafts are untouched.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpDelete("api/sheets/{id:int}/drafts")]
    [Authorize(Policy = PermissionKeys.SheetsEdit)]
    public async Task<ActionResult<SheetDto>> DiscardDrafts(int id, CancellationToken cancellationToken)
    {
        var result = await sheets.DiscardDraftsAsync(id, cancellationToken);
        return Ok(result);
    }
}
