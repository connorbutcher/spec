using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Api.Published;
using PUSpecSheet.Application.Common;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Contracts.Published;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Rows of a published sheet by row identifier, for an application that stores the identifiers of the rows
/// it needs. One call returns any number of rows, each read straight out of the answer by its identifier.
/// </summary>
[ApiController]
[Tags(ApiTags.PublishedSheets)]
[Route("api/published/sheets/{sheetPublicId:guid}/versions")]
public sealed class PublishedRowsController(IPublishedSheetQueryService sheets, IPublishedRowsService rows) : ControllerBase
{
    /// <summary>Read rows by identifier at a version</summary>
    /// <remarks>
    /// Returns <c>rows</c> as an object with one entry per row, named by the row's identifier, so
    /// <c>rows["{rowId}"]</c> is that row. With <c>ids</c> only those rows are returned, in one request
    /// however many there are; without it every row of the sheet is. Identifiers the version doesn't hold
    /// are listed in <c>missing</c>.
    ///
    /// Each row has <c>values</c>: its own cells, left to right, with null for an empty cell. A row of a
    /// table with a set of columns per part also has <c>columns</c>, with the row's values for each part
    /// under the part number: <c>rows["{rowId}"].columns["P-1003"]</c>. <c>columns=P-1003</c> keeps only
    /// that part.
    ///
    /// The answer for a version number never changes, so it is sent with <c>Cache-Control: immutable</c>.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="version" example="3">The version number, starting at 1.</param>
    /// <param name="ids">Comma-separated row identifiers. Leave out for every row.</param>
    /// <param name="columns" example="P-1003">Comma-separated names of the sets of columns to keep, such as part numbers. Leave out for all of them.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The rows, by identifier.</response>
    /// <response code="404">The sheet doesn't exist or has no such version.</response>
    [HttpGet("{version:int}/rows")]
    [ProducesResponseType<PublishedRowsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedRowsDto>> AtVersion(
        Guid sheetPublicId,
        int version,
        [FromQuery] string? ids,
        [FromQuery] string? columns,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Version(version), PublishedRowsSelection.Parse(ids, columns), cancellationToken);
    }

    /// <summary>Read rows by identifier at the newest version</summary>
    /// <remarks>
    /// The same as reading rows at a version, at the sheet's newest version. Send the last <c>ETag</c> in
    /// <c>If-None-Match</c> to get 304 when nothing was published since. <c>Content-Location</c> names the
    /// version that was returned.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="ids">Comma-separated row identifiers. Leave out for every row.</param>
    /// <param name="columns" example="P-1003">Comma-separated names of the sets of columns to keep, such as part numbers. Leave out for all of them.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The rows, by identifier.</response>
    /// <response code="404">The sheet doesn't exist or nothing has been published on it.</response>
    [HttpGet("latest/rows")]
    [ProducesResponseType<PublishedRowsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedRowsDto>> Latest(
        Guid sheetPublicId,
        [FromQuery] string? ids,
        [FromQuery] string? columns,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Latest, PublishedRowsSelection.Parse(ids, columns), cancellationToken);
    }

    /// <summary>Read many rows by identifier at a version</summary>
    /// <remarks>
    /// The same as reading rows at a version, with the identifiers in the body for lists too long for a
    /// query string, such as several hundred rows. It changes nothing, returns the same <c>ETag</c> and
    /// answers 304 to a matching <c>If-None-Match</c>.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="version" example="3">The version number, starting at 1.</param>
    /// <param name="request">The rows to read and the sets of columns to keep.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The rows, by identifier.</response>
    /// <response code="404">The sheet doesn't exist or has no such version.</response>
    [HttpPost("{version:int}/rows/query")]
    [ProducesResponseType<PublishedRowsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedRowsDto>> Query(
        Guid sheetPublicId,
        int version,
        PublishedRowsQueryRequest request,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Version(version), PublishedRowsSelection.From(request), cancellationToken);
    }

    /// <summary>Read many rows by identifier at the newest version</summary>
    /// <remarks>
    /// The same as reading many rows at a version, at the sheet's newest version.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="request">The rows to read and the sets of columns to keep.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The rows, by identifier.</response>
    /// <response code="404">The sheet doesn't exist or nothing has been published on it.</response>
    [HttpPost("latest/rows/query")]
    [ProducesResponseType<PublishedRowsDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedRowsDto>> QueryLatest(
        Guid sheetPublicId,
        PublishedRowsQueryRequest request,
        CancellationToken cancellationToken)
    {
        return ReadAsync(sheetPublicId, PublishedVersionPoint.Latest, PublishedRowsSelection.From(request), cancellationToken);
    }

    /// <summary>Read one row at a version</summary>
    /// <remarks>
    /// One row by its identifier: its <c>values</c> and, on a table with a set of columns per part, its
    /// <c>columns</c> by part number. With <c>columns=P-1003</c> the answer is that row's values for that
    /// part alone.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="version" example="3">The version number, starting at 1.</param>
    /// <param name="rowPublicId">The row's public identifier.</param>
    /// <param name="columns" example="P-1003">Comma-separated names of the sets of columns to keep, such as part numbers. Leave out for all of them.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The row.</response>
    /// <response code="404">The sheet doesn't exist, has no such version, or the version doesn't hold the row.</response>
    [HttpGet("{version:int}/rows/{rowPublicId:guid}")]
    [ProducesResponseType<PublishedSheetRowDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedSheetRowDto>> RowAtVersion(
        Guid sheetPublicId,
        int version,
        Guid rowPublicId,
        [FromQuery] string? columns,
        CancellationToken cancellationToken)
    {
        return ReadRowAsync(sheetPublicId, PublishedVersionPoint.Version(version), rowPublicId, columns, cancellationToken);
    }

    /// <summary>Read one row at the newest version</summary>
    /// <remarks>
    /// The same as reading one row at a version, at the sheet's newest version.
    /// </remarks>
    /// <param name="sheetPublicId">The sheet's public identifier.</param>
    /// <param name="rowPublicId">The row's public identifier.</param>
    /// <param name="columns" example="P-1003">Comma-separated names of the sets of columns to keep, such as part numbers. Leave out for all of them.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The row.</response>
    /// <response code="404">The sheet doesn't exist, nothing has been published on it, or its newest version doesn't hold the row.</response>
    [HttpGet("latest/rows/{rowPublicId:guid}")]
    [ProducesResponseType<PublishedSheetRowDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    public Task<ActionResult<PublishedSheetRowDto>> RowAtLatest(
        Guid sheetPublicId,
        Guid rowPublicId,
        [FromQuery] string? columns,
        CancellationToken cancellationToken)
    {
        return ReadRowAsync(sheetPublicId, PublishedVersionPoint.Latest, rowPublicId, columns, cancellationToken);
    }

    private async Task<ActionResult<PublishedRowsDto>> ReadAsync(
        Guid sheetPublicId,
        PublishedVersionPoint point,
        PublishedRowsSelection selection,
        CancellationToken cancellationToken)
    {
        var version = await sheets.ResolveAsync(sheetPublicId, point, cancellationToken);
        if (CallerHas(version, point, selection, "rows"))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var result = await rows.ReadAsync(version, selection, cancellationToken);
        return Ok(result);
    }

    private async Task<ActionResult<PublishedSheetRowDto>> ReadRowAsync(
        Guid sheetPublicId,
        PublishedVersionPoint point,
        Guid rowPublicId,
        string? columns,
        CancellationToken cancellationToken)
    {
        var selection = PublishedRowsSelection.One(rowPublicId, columns);
        var version = await sheets.ResolveAsync(sheetPublicId, point, cancellationToken);
        if (CallerHas(version, point, selection, $"rows/{rowPublicId}"))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        var result = await rows.ReadAsync(version, selection, cancellationToken);
        if (!result.Rows.TryGetValue(rowPublicId, out var row))
        {
            throw new NotFoundException($"Version {version.VersionNumber} of sheet {sheetPublicId} has no row {rowPublicId}.");
        }

        return Ok(new PublishedSheetRowDto(
            result.Sheet,
            result.Version,
            result.PublishedAtUtc,
            rowPublicId,
            row.Section,
            row.Values,
            row.Columns));
    }

    /// <summary>Sets the caching headers, and says whether the caller already holds this answer.</summary>
    private bool CallerHas(ResolvedSheetVersion version, PublishedVersionPoint point, PublishedRowsSelection selection, string path)
    {
        var etag = new EntityTagHeaderValue($"\"{version.SheetPublicId:N}-v{version.VersionNumber}-{selection.Key}\"");
        PublishedSheetHttpCache.Apply(Response, etag, isFixed: point.VersionNumber is not null);

        if (point.VersionNumber is null)
        {
            PublishedSheetHttpCache.PointAtVersion(Request, Response, version, path);
        }

        return PublishedSheetHttpCache.CallerHas(Request, etag);
    }
}
