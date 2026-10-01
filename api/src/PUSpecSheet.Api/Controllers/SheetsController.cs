using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Opening, publishing and discarding sheets. Every action returns the sheet as the current user sees it.
/// A sheet is shown live, as of a version number (<c>version</c>) or as of a date and time (<c>asOf</c>).
/// </summary>
[ApiController]
public sealed class SheetsController(ISheetService sheets, ISheetTableService tables) : ControllerBase
{
    /// <summary>Opens the sheet of a sheet type for a phase, creating it empty the first time.</summary>
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

    [HttpPost("api/sheets/{id:int}/tables")]
    public async Task<ActionResult<SheetDto>> AddTable(int id, AddSheetTableRequest request, CancellationToken cancellationToken)
    {
        var result = await tables.AddAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("api/sheets/{id:int}/publish")]
    public async Task<ActionResult<SheetDto>> Publish(int id, PublishSheetRequest request, CancellationToken cancellationToken)
    {
        var result = await sheets.PublishAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("api/sheets/{id:int}/drafts")]
    public async Task<ActionResult<SheetDto>> DiscardDrafts(int id, CancellationToken cancellationToken)
    {
        var result = await sheets.DiscardDraftsAsync(id, cancellationToken);
        return Ok(result);
    }
}
