using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Approvals;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Opening, publishing and discarding sheets. Every action returns the sheet as the current user sees it.
/// A sheet is shown live, as of a version number (<c>version</c>) or as of a date and time (<c>asOf</c>).
/// </summary>
[ApiController]
[Tags(ApiTags.Sheets)]
public sealed class SheetsController(ISheetService sheets, ISheetTableService tables, ISheetSubmissionService submissions) : ControllerBase
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
    /// The sheet with this id, live or as of a version number or a date and time, or with a submission's
    /// changes on top for its approvers to check. Only one of <c>version</c>, <c>asOf</c> and <c>submission</c> can be given.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="version" example="3">Show the sheet as of this version number. Leave out for the live view.</param>
    /// <param name="asOf" example="2026-09-30T14:02:11Z">Show the sheet as it was published at this date and time (UTC). Can't be combined with <c>version</c>.</param>
    /// <param name="submission" example="12">Show the live sheet with this submission's changes, which wait for approval, on top. Read-only, and only for its submitter and approvers.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet as you see it.</response>
    /// <response code="403">You aren't the submission's submitter or one of its approvers.</response>
    /// <response code="404">The sheet, version or submission doesn't exist.</response>
    /// <response code="409">The submission has already been decided.</response>
    [HttpGet("api/sheets/{id:int}")]
    public async Task<ActionResult<SheetDto>> Get(
        int id,
        [FromQuery] int? version,
        [FromQuery] DateTime? asOf,
        [FromQuery] int? submission,
        CancellationToken cancellationToken)
    {
        var result = await sheets.GetAsync(id, SheetViewPoint.From(version, asOf, submission), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get some rows of a sheet</summary>
    /// <remarks>
    /// Rows of the live sheet as you see them, with your draft count, other people's drafts, the choices
    /// of linked dropdowns and the rows of their tables that can be linked to. Read this in place of the
    /// whole sheet when the live connection says only some rows changed. A row asked for that isn't on the
    /// sheet any more is left out.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="ids">The rows to read, by id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The rows as you see them.</response>
    /// <response code="400">No rows, or more than 500, were asked for.</response>
    /// <response code="404">The sheet doesn't exist.</response>
    [HttpGet("api/sheets/{id:int}/rows")]
    public async Task<ActionResult<SheetRowsChangeDto>> GetRows(
        int id,
        [FromQuery, MinLength(1), MaxLength(500)] int[] ids,
        CancellationToken cancellationToken)
    {
        var result = await sheets.GetRowsAsync(id, ids, cancellationToken);
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

    /// <summary>Publish drafts, once approved</summary>
    /// <remarks>
    /// Submits drafts on the sheet for publishing as its next version, with an optional note, to one or more
    /// approvers (see <c>GET api/approvers</c>). Nothing is published yet: the drafts stay locked, to you as
    /// well, and stay out of the published sheet until every approver approves; the last approval publishes
    /// them. A rejection hands them back to their authors to change and submit again.
    /// With <c>scope</c> <c>Mine</c> (the default) only your own drafts go in and other people's are untouched.
    /// With <c>All</c> every draft on the sheet at that moment goes in, whoever made it: each change stays
    /// attributed to the person who made it, and the version to you.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="request">The approvers, an optional note describing the version, and whose drafts to publish.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="400">There is nothing to publish, required cells are still empty, or an approver can't be chosen.</response>
    /// <response code="409">Someone changed one of the drafts at the same moment.</response>
    [HttpPost("api/sheets/{id:int}/publish")]
    [Authorize(Policy = PermissionKeys.SheetsPublish)]
    public async Task<ActionResult<SheetDto>> Publish(int id, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        var result = await submissions.SubmitAsync(id, request, cancellationToken);
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
