using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Api.Published;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Finds published data by the value of a cell that a template author gave a lookup key, such as a part
/// number, for other applications. Read-only, values only, and never drafts.
/// </summary>
[ApiController]
[Tags(ApiTags.PublishedSheets)]
[Route("api/published")]
public sealed class PublishedLookupController(IPublishedLookupService lookup) : ControllerBase
{
    /// <summary>Look a value up on every sheet</summary>
    /// <remarks>
    /// Finds every published sheet where a cell with the lookup key holds the value, and returns what
    /// belongs to it on each. A key is given to a template cell in the template editor, or with
    /// <c>PUT /api/template-cells/{id}/lookup-key</c>, and works on any sheet type and any table layout.
    ///
    /// Each match has <c>rows</c> by row identifier, in the same shape as reading rows by identifier. A
    /// value at the top of a part's columns, such as a part number, brings every row of the table with
    /// the part's values under <c>columns["P-1003"]</c> and the row's own cells, such as its description,
    /// in <c>values</c>. A value in one of a row's own cells brings the rows of its section and
    /// sub-sections, or of the whole table when the cell is in the header.
    ///
    /// Each sheet is read at its newest version, or with <c>at</c> as it stood at that moment. The value is
    /// matched whole, ignoring case. A value found in several sheets, tables or columns gives one match for
    /// each; a value found nowhere gives an empty <c>matches</c> list. Send the last <c>ETag</c> in
    /// <c>If-None-Match</c> to get 304 when nothing that matches has been published since.
    /// </remarks>
    /// <param name="key" example="partNumber">The lookup key of the cells to search.</param>
    /// <param name="value" example="P-1003">The value to find.</param>
    /// <param name="phase" example="V6">Only search the sheets of the phase with this code.</param>
    /// <param name="sheetType" example="3">Only search sheets of this sheet type, by its id from <c>GET /api/sheet-types</c>.</param>
    /// <param name="at" example="2026-09-30T14:02:11Z">Read each sheet as it stood at this ISO 8601 date and time. Without an offset it is taken as UTC.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Every place the value was found, with what belongs to it.</response>
    /// <response code="400">The key isn't one any template cell has, the value is empty, or <c>at</c> isn't a date and time.</response>
    [HttpGet("lookup")]
    [ProducesResponseType<PublishedLookupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedLookupDto>> Find(
        [FromQuery] string key,
        [FromQuery] string value,
        [FromQuery] string? phase,
        [FromQuery] int? sheetType,
        [FromQuery] string? at,
        CancellationToken cancellationToken)
    {
        var point = PublishedVersionPoint.Latest;
        if (!string.IsNullOrWhiteSpace(at))
        {
            point = PublishedVersionPoint.At(PublishedMoment.ParseUtc(at));
        }

        var criteria = PublishedLookupCriteria.Create(key, value, phase, sheetType);
        return ReadAsync(criteria, null, point, cancellationToken);
    }

    /// <summary>Look a value up on a sheet at a version</summary>
    /// <remarks>
    /// The same as looking a value up on every sheet, kept to one sheet at one version number.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="version" example="3">The version number, starting at 1.</param>
    /// <param name="key" example="partNumber">The lookup key of the cells to search.</param>
    /// <param name="value" example="P-1003">The value to find.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Every place on the sheet the value was found at that version.</response>
    /// <response code="400">The key isn't one any template cell has, or the value is empty.</response>
    /// <response code="404">The sheet doesn't exist or has no such version.</response>
    [HttpGet("sheets/{sheetPublicId:guid}/versions/{version:int}/lookup")]
    [ProducesResponseType<PublishedLookupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedLookupDto>> FindAtVersion(
        Guid sheetPublicId,
        int version,
        [FromQuery] string key,
        [FromQuery] string value,
        CancellationToken cancellationToken)
    {
        var criteria = PublishedLookupCriteria.Create(key, value);
        return ReadAsync(criteria, sheetPublicId, PublishedVersionPoint.Version(version), cancellationToken);
    }

    /// <summary>Look a value up on a sheet at its newest version</summary>
    /// <remarks>
    /// The same as looking a value up on every sheet, kept to one sheet at its newest version.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="key" example="partNumber">The lookup key of the cells to search.</param>
    /// <param name="value" example="P-1003">The value to find.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">Every place on the sheet the value was found at its newest version.</response>
    /// <response code="400">The key isn't one any template cell has, or the value is empty.</response>
    /// <response code="404">The sheet doesn't exist or nothing has been published on it.</response>
    [HttpGet("sheets/{sheetPublicId:guid}/versions/latest/lookup")]
    [ProducesResponseType<PublishedLookupDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedLookupDto>> FindAtLatest(
        Guid sheetPublicId,
        [FromQuery] string key,
        [FromQuery] string value,
        CancellationToken cancellationToken)
    {
        var criteria = PublishedLookupCriteria.Create(key, value);
        return ReadAsync(criteria, sheetPublicId, PublishedVersionPoint.Latest, cancellationToken);
    }

    private async Task<ActionResult<PublishedLookupDto>> ReadAsync(
        PublishedLookupCriteria criteria,
        Guid? sheetPublicId,
        PublishedVersionPoint point,
        CancellationToken cancellationToken)
    {
        var resolution = await lookup.ResolveAsync(criteria, sheetPublicId, point, cancellationToken);
        var etag = new EntityTagHeaderValue($"\"{resolution.Key}\"");

        // A lookup key can be given to a cell or taken away at any time, so even an answer for a fixed
        // version is checked before it is reused. The check is the two small queries above.
        PublishedSheetHttpCache.Apply(Response, etag, isFixed: false);

        if (PublishedSheetHttpCache.CallerHas(Request, etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var result = await lookup.ReadAsync(resolution, cancellationToken);
        return Ok(result);
    }
}
