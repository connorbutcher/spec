using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Api.Published;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Published sheet values for other applications: read-only, values only, and never drafts. A sheet is
/// read at a version number, at the newest version, or as it stood at a moment; all three resolve to one
/// version, which is the ETag, so an unchanged answer costs one small query.
/// </summary>
[ApiController]
[Tags(ApiTags.PublishedSheets)]
[Route("api/published/sheets")]
public sealed class PublishedSheetsController(IPublishedSheetQueryService sheets) : ControllerBase
{
    /// <summary>Find a sheet</summary>
    /// <remarks>
    /// Finds a sheet's identifier and newest version from its phase code and sheet type. Do this once and
    /// keep the identifier: it never changes. <c>latestVersion</c> is null for a sheet nothing has been
    /// published on yet.
    /// </remarks>
    /// <param name="phase" example="V6">The phase's code, as shown in the phase tree.</param>
    /// <param name="sheetType" example="1">The sheet type's id, from <c>GET /api/sheet-types</c>.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet's identifier and its newest version number.</response>
    /// <response code="404">No phase has this code, or the phase has no sheet of this type.</response>
    [HttpGet]
    public async Task<ActionResult<PublishedSheetReferenceDto>> Find(
        [FromQuery] string phase,
        [FromQuery] int sheetType,
        CancellationToken cancellationToken)
    {
        var result = await sheets.FindAsync(phase, sheetType, cancellationToken);
        return Ok(result);
    }

    /// <summary>List a sheet's versions</summary>
    /// <remarks>
    /// Every published version of the sheet, oldest first, with when it was published and its note.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The versions, oldest first. Empty when nothing has been published.</response>
    [HttpGet("{sheetPublicId:guid}/versions")]
    public async Task<ActionResult<IReadOnlyList<PublishedVersionDto>>> Versions(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var result = await sheets.VersionsAsync(sheetPublicId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Read a sheet at a version</summary>
    /// <remarks>
    /// The sheet's values at a version number. The answer never changes, so it is sent with
    /// <c>Cache-Control: immutable</c> and callers can keep it for good.
    ///
    /// With no selection the whole sheet is returned. <c>tables</c>, <c>sections</c>, <c>rows</c> and
    /// <c>cells</c> narrow it and are combined: a table or section brings everything beneath it. Identifiers
    /// that the version doesn't hold are listed in <c>missing</c> rather than failing the request.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="version" example="3">The version number, starting at 1.</param>
    /// <param name="options">What to read and how to lay it out.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The selected values at that version.</response>
    /// <response code="404">The sheet doesn't exist or has no such version.</response>
    [HttpGet("{sheetPublicId:guid}/versions/{version:int}")]
    [ProducesResponseType<PublishedSheetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedSheetDto>> AtVersion(
        Guid sheetPublicId,
        int version,
        [FromQuery] PublishedSheetQueryOptions options,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Version(version), options.ToSelection(), true, cancellationToken);
    }

    /// <summary>Read a sheet at its newest version</summary>
    /// <remarks>
    /// The sheet's values at its newest version. Send the last <c>ETag</c> in <c>If-None-Match</c> to get
    /// 304 when nothing was published since, which costs one small query. <c>Content-Location</c> names the
    /// version that was returned.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="options">What to read and how to lay it out.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The selected values at the newest version.</response>
    /// <response code="404">The sheet doesn't exist or nothing has been published on it.</response>
    [HttpGet("{sheetPublicId:guid}/versions/latest")]
    [ProducesResponseType<PublishedSheetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedSheetDto>> Latest(
        Guid sheetPublicId,
        [FromQuery] PublishedSheetQueryOptions options,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Latest, options.ToSelection(), false, cancellationToken);
    }

    /// <summary>Read a sheet as it stood at a moment</summary>
    /// <remarks>
    /// The sheet's values at the newest version published at or before a moment. A past moment's answer is
    /// settled and is sent as immutable; a future moment can still gain a version, so it is revalidated.
    /// <c>Content-Location</c> names the version that was returned.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="instant" example="2026-09-30T14:02:11Z">An ISO 8601 date and time. Without an offset it is taken as UTC.</param>
    /// <param name="options">What to read and how to lay it out.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The selected values as they stood at that moment.</response>
    /// <response code="400">The moment isn't a date and time.</response>
    /// <response code="404">The sheet doesn't exist or had nothing published by that moment.</response>
    [HttpGet("{sheetPublicId:guid}/versions/at/{instant}")]
    [ProducesResponseType<PublishedSheetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedSheetDto>> AtMoment(
        Guid sheetPublicId,
        string instant,
        [FromQuery] PublishedSheetQueryOptions options,
        CancellationToken cancellationToken)
    {
        // What a sheet held at a past moment is settled; a future moment can still gain a version.
        var moment = PublishedMoment.ParseUtc(instant);
        var isPast = moment <= DateTime.UtcNow;
        return ReadAsync(sheetPublicId, PublishedVersionPoint.At(moment), options.ToSelection(), isPast, cancellationToken);
    }

    /// <summary>Read a sheet at a version with a long selection</summary>
    /// <remarks>
    /// The same as reading a version, with the selection in the body for lists too long for a query string.
    /// It changes nothing, returns the same <c>ETag</c> and answers 304 to a matching <c>If-None-Match</c>.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="version" example="3">The version number, starting at 1.</param>
    /// <param name="request">What to read and how to lay it out.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The selected values at that version.</response>
    /// <response code="404">The sheet doesn't exist or has no such version.</response>
    [HttpPost("{sheetPublicId:guid}/versions/{version:int}/query")]
    [ProducesResponseType<PublishedSheetDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedSheetDto>> Query(
        Guid sheetPublicId,
        int version,
        PublishedSheetQueryRequest request,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Version(version), PublishedSheetSelection.From(request), true, cancellationToken);
    }

    private async Task<ActionResult<PublishedSheetDto>> ReadAsync(
        Guid sheetPublicId,
        PublishedVersionPoint point,
        PublishedSheetSelection selection,
        bool isFixed,
        CancellationToken cancellationToken)
    {
        var version = await sheets.ResolveAsync(sheetPublicId, point, cancellationToken);
        var etag = PublishedSheetHttpCache.ETagFor(version, selection);
        PublishedSheetHttpCache.Apply(Response, etag, isFixed);

        if (point.VersionNumber is null)
        {
            PublishedSheetHttpCache.PointAtVersion(Request, Response, version, null);
        }

        if (PublishedSheetHttpCache.CallerHas(Request, etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var result = await sheets.ReadAsync(version, selection, cancellationToken);
        return Ok(result);
    }
}
