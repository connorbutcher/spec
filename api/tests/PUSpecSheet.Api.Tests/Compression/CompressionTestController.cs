using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using PUSpecSheet.Api.Published;

namespace PUSpecSheet.Api.Tests.Compression;

/// <summary>Controller responses of the kinds the real API sends, with no database behind them.</summary>
[ApiController]
[Route("test")]
public sealed class CompressionTestController : ControllerBase
{
    /// <summary>A strong ETag, like the published sheet endpoints send.</summary>
    public static readonly EntityTagHeaderValue Tag = new("\"v3-selection\"");

    [HttpGet("rows")]
    public IActionResult Rows()
    {
        var rows = Enumerable.Range(1, 400).Select(index => new
        {
            id = index,
            description = $"Bore diameter {index} (mm)",
            lower = 81.98m + (index % 7 * 0.01m),
            upper = 82.02m + (index % 7 * 0.01m),
        });
        return Ok(rows);
    }

    [HttpGet("tiny")]
    public IActionResult Tiny()
    {
        return Ok(new { ok = true });
    }

    [HttpGet("problem")]
    public IActionResult Problem404()
    {
        return Problem(
            title: "Sheet 7 was not found.",
            detail: new string('x', 600),
            statusCode: StatusCodes.Status404NotFound);
    }

    [HttpGet("image")]
    public IActionResult Image()
    {
        return File(new byte[4000], "image/png");
    }

    [HttpGet("published")]
    public IActionResult Published()
    {
        PublishedSheetHttpCache.Apply(Response, Tag, isFixed: true);
        if (PublishedSheetHttpCache.CallerHas(Request, Tag))
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        return Rows();
    }
}
