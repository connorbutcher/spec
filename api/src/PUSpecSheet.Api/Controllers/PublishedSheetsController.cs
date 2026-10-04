using System.Globalization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.Published;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Published sheet values for other applications: read-only, values only, and never drafts. A sheet is
/// read at a version number, at the newest version, or as it stood at a moment; all three resolve to one
/// version, which is the ETag, so an unchanged answer costs one small query.
/// </summary>
[ApiController]
[Route("api/published/sheets")]
public sealed class PublishedSheetsController(IPublishedSheetQueryService sheets) : ControllerBase
{
    /// <summary>Finds a sheet's identifier and newest version from its phase code and sheet type.</summary>
    [HttpGet]
    public async Task<ActionResult<PublishedSheetReferenceDto>> Find(
        [FromQuery] string phase,
        [FromQuery] int sheetType,
        CancellationToken cancellationToken)
    {
        var result = await sheets.FindAsync(phase, sheetType, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{sheetPublicId:guid}/versions")]
    public async Task<ActionResult<IReadOnlyList<PublishedVersionDto>>> Versions(Guid sheetPublicId, CancellationToken cancellationToken)
    {
        var result = await sheets.VersionsAsync(sheetPublicId, cancellationToken);
        return Ok(result);
    }

    /// <summary>The sheet at a version number. The answer never changes, so callers can keep it for good.</summary>
    [HttpGet("{sheetPublicId:guid}/versions/{version:int}")]
    public Task<ActionResult<PublishedSheetDto>> AtVersion(
        Guid sheetPublicId,
        int version,
        [FromQuery] PublishedSheetQueryOptions options,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Version(version), options.ToSelection(), true, cancellationToken);
    }

    /// <summary>The sheet at its newest version. Send the last ETag in <c>If-None-Match</c> to get 304 when nothing was published since.</summary>
    [HttpGet("{sheetPublicId:guid}/versions/latest")]
    public Task<ActionResult<PublishedSheetDto>> Latest(
        Guid sheetPublicId,
        [FromQuery] PublishedSheetQueryOptions options,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Latest, options.ToSelection(), false, cancellationToken);
    }

    /// <summary>The sheet as it stood at a moment, e.g. <c>2026-09-30T14:02:11Z</c>. A time without an offset is taken as UTC.</summary>
    [HttpGet("{sheetPublicId:guid}/versions/at/{instant}")]
    public Task<ActionResult<PublishedSheetDto>> AtMoment(
        Guid sheetPublicId,
        string instant,
        [FromQuery] PublishedSheetQueryOptions options,
        CancellationToken cancellationToken)
    {
        if (!DateTimeOffset.TryParse(instant, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsed))
        {
            throw new InvalidRequestException($"\"{instant}\" is not a date and time.");
        }

        // What a sheet held at a past moment is settled; a future moment can still gain a version.
        var moment = parsed.UtcDateTime;
        var isPast = moment <= DateTime.UtcNow;
        return ReadAsync(sheetPublicId, PublishedVersionPoint.At(moment), options.ToSelection(), isPast, cancellationToken);
    }

    /// <summary>
    /// The same as reading a version, with the selection in the body for lists too long for a query string.
    /// It returns the same ETag and answers 304 to a matching <c>If-None-Match</c>.
    /// </summary>
    [HttpPost("{sheetPublicId:guid}/versions/{version:int}/query")]
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
            // Tells the caller which version it got, as the address that always returns the same answer.
            Response.Headers.ContentLocation =
                $"{Request.PathBase}/api/published/sheets/{sheetPublicId}/versions/{version.VersionNumber}{Request.QueryString}";
        }

        if (PublishedSheetHttpCache.CallerHas(Request, etag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var result = await sheets.ReadAsync(version, selection, cancellationToken);
        return Ok(result);
    }
}
