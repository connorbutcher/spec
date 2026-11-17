using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Api.Published;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Contracts.Approvals;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers.Sheets;

/// <summary>
/// Finding, reading and submitting sheets. Each way of looking at a sheet has its own endpoint: live (the
/// latest published state with your own drafts on top), as a version was published, as it stood at a
/// moment, and with a submission's changes on top for its approvers to check. Only the live view can be edited.
/// </summary>
[ApiController]
[Tags(ApiTags.Sheets)]
public sealed class SheetsController : ControllerBase
{
    private readonly ISheetsService sheets;

    public SheetsController(ISheetsService sheets)
    {
        this.sheets = sheets;
    }

    /// <summary>Open a phase's sheet of a type</summary>
    /// <remarks>
    /// The live view of the phase's one sheet of a sheet type that has no identifier values, creating it
    /// empty the first time. A sheet type whose identifiers are required can have many sheets in a phase,
    /// none of them found this way: list them with <c>GET api/phases/{phaseId}/sheets</c> and read them by id.
    /// For a past view, read the sheet by the id this returns.
    /// </remarks>
    /// <param name="phaseId">The phase's id.</param>
    /// <param name="sheetTypeId">The sheet type's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The live sheet as you see it.</response>
    /// <response code="404">The phase doesn't exist, the sheet type isn't available to it, or the type needs identifiers.</response>
    [HttpGet("api/phases/{phaseId:int}/sheets/{sheetTypeId:int}")]
    public async Task<ActionResult<SheetDto>> Open(int phaseId, int sheetTypeId, CancellationToken cancellationToken)
    {
        var result = await sheets.OpenAsync(phaseId, sheetTypeId, cancellationToken);
        return Ok(result);
    }

    /// <summary>List a phase's sheets</summary>
    /// <remarks>
    /// Every sheet of the phase, of every sheet type, oldest first: its identifier values (see
    /// <c>GET api/sheet-types</c> for what each identifier is), its latest version, how many unpublished
    /// changes it has and how many submissions wait for approval on it. A sheet type without identifiers has
    /// at most one sheet per phase, and none until it is first opened.
    /// </remarks>
    /// <param name="phaseId">The phase's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The phase's sheets.</response>
    /// <response code="404">The phase doesn't exist.</response>
    [HttpGet("api/phases/{phaseId:int}/sheets")]
    public async Task<ActionResult<IReadOnlyList<SheetSummaryDto>>> List(int phaseId, CancellationToken cancellationToken)
    {
        var result = await sheets.ListAsync(phaseId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a sheet</summary>
    /// <remarks>
    /// Creates a sheet of a sheet type in a phase, with a value for each of the type's identifiers (an
    /// engine specification's serial number, say). The values tell it from the phase's other sheets of the type
    /// and can't be changed afterwards. With <c>copyFromSheetId</c> the sheet starts with the latest
    /// published content of that sheet, which must be of the same type, as your drafts.
    /// </remarks>
    /// <param name="phaseId">The phase's id.</param>
    /// <param name="request">The sheet type, the identifier values and the sheet to copy, if any.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The new sheet.</response>
    /// <response code="400">A required identifier is missing, a value isn't of its identifier's kind, or the sheet to copy is of another type.</response>
    /// <response code="404">The phase doesn't have the sheet type, or the sheet to copy doesn't exist.</response>
    /// <response code="409">The phase already has a sheet of the type with these identifier values.</response>
    [HttpPost("api/phases/{phaseId:int}/sheets")]
    [Authorize(Policy = PermissionKeys.SheetsEdit)]
    public async Task<ActionResult<SheetSummaryDto>> Create(int phaseId, CreateSheetRequest request, CancellationToken cancellationToken)
    {
        var result = await sheets.CreateAsync(phaseId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a sheet</summary>
    /// <remarks>
    /// The live view of the sheet: the latest published state with your own drafts on top and other people's
    /// locks marked. This is the view that can be edited.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The live sheet as you see it.</response>
    /// <response code="404">The sheet doesn't exist.</response>
    [HttpGet("api/sheets/{id:int}")]
    public async Task<ActionResult<SheetDto>> Get(int id, CancellationToken cancellationToken)
    {
        var result = await sheets.GetAsync(id, SheetViewPoint.Live, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a sheet as a version was published</summary>
    /// <remarks>
    /// The sheet as it stood when the version was published, read-only, with no drafts.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="version" example="3">The version number, starting at 1.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet at that version.</response>
    /// <response code="404">The sheet or the version doesn't exist.</response>
    [HttpGet("api/sheets/{id:int}/versions/{version:int}")]
    public async Task<ActionResult<SheetDto>> GetVersion(int id, int version, CancellationToken cancellationToken)
    {
        var result = await sheets.GetAsync(id, SheetViewPoint.AtVersion(version), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a sheet as it stood at a moment</summary>
    /// <remarks>
    /// The sheet as it was published at a date and time, read-only, with no drafts: the version that was
    /// newest then.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="instant" example="2026-09-30T14:02:11Z">An ISO 8601 date and time. Without an offset it is taken as UTC.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet at that moment.</response>
    /// <response code="400">The moment isn't a date and time.</response>
    /// <response code="404">The sheet doesn't exist.</response>
    [HttpGet("api/sheets/{id:int}/versions/at/{instant}")]
    public async Task<ActionResult<SheetDto>> GetAtMoment(int id, string instant, CancellationToken cancellationToken)
    {
        var result = await sheets.GetAsync(id, SheetViewPoint.AsOf(PublishedMoment.ParseUtc(instant)), cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a submission's sheet with its changes</summary>
    /// <remarks>
    /// The live sheet the submission is on, with the submission's changes, which wait for approval, on top,
    /// so its approvers can check them. Read-only, only for its submitter and approvers, and only while it waits.
    /// </remarks>
    /// <param name="submissionId">The submission's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet with the submission's changes.</response>
    /// <response code="403">You aren't the submission's submitter or one of its approvers.</response>
    /// <response code="404">The submission doesn't exist.</response>
    /// <response code="409">The submission has already been decided.</response>
    [HttpGet("api/submissions/{submissionId:int}/sheet")]
    public async Task<ActionResult<SheetDto>> GetForReview(int submissionId, CancellationToken cancellationToken)
    {
        var result = await sheets.GetForReviewAsync(submissionId, cancellationToken);
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
        var result = await sheets.AddTableAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Submit drafts for approval</summary>
    /// <remarks>
    /// Submits drafts on the sheet to one or more approvers (see <c>GET api/approvers</c>) to be published
    /// as its next version, with an optional note. Nothing is published yet: the drafts stay locked, to you as
    /// well, and stay out of the published sheet until every approver approves; the last approval publishes
    /// them. A rejection hands them back to their authors to change and submit again.
    /// With <c>scope</c> <c>Mine</c> (the default) only your own drafts go in and other people's are untouched.
    /// With <c>All</c> every draft on the sheet at that moment goes in, whoever made it: each change stays
    /// attributed to the person who made it, and the version to you.
    /// </remarks>
    /// <param name="id">The sheet's id.</param>
    /// <param name="request">The approvers, an optional note describing the version, and whose drafts to submit.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="400">There is nothing to submit, required cells are still empty, or an approver can't be chosen.</response>
    /// <response code="409">Someone changed one of the drafts at the same moment.</response>
    [HttpPost("api/sheets/{id:int}/submissions")]
    [Authorize(Policy = PermissionKeys.SheetsPublish)]
    public async Task<ActionResult<SheetDto>> Submit(int id, SubmitSheetRequest request, CancellationToken cancellationToken)
    {
        var result = await sheets.SubmitAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
