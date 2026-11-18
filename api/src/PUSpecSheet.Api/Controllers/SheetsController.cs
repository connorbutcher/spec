using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Sheets in the shape of the revision schema, alongside the legacy <c>specsheets</c> endpoints. Ids here are sheet ids
/// and the ids of the sheet's items, not those of a copy.
/// </summary>
[Route("sheets")]
[Authorize]
public class SheetsController : ControllerBase
{
    private readonly ISheetDataAccessService _sheetDataAccessService;

    public SheetsController(ISheetDataAccessService sheetDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(sheetDataAccessService, nameof(sheetDataAccessService));

        _sheetDataAccessService = sheetDataAccessService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Sheet>> GetSheetAsync(int id, [FromQuery] int? version = null)
    {
        var sheet = await _sheetDataAccessService.GetPublishedSheetAsync(id, version, HttpContext.RequestAborted);
        return sheet is null ? NotFound() : Ok(sheet);
    }

    [HttpGet("{id:int}/draft")]
    public async Task<ActionResult<Sheet>> GetDraftSheetAsync(int id)
    {
        var sheet = await _sheetDataAccessService.GetDraftSheetAsync(id, HttpContext.RequestAborted);
        return sheet is null ? NotFound() : Ok(sheet);
    }

    [HttpGet("phase/{phaseId:int}/all")]
    public async Task<ActionResult<IReadOnlyList<SheetSummary>>> GetSheetSummariesByPhaseAsync(int phaseId, [FromQuery] bool includeChildren = false)
    {
        var summaries = await _sheetDataAccessService.GetSheetSummariesByPhaseAsync(phaseId, includeChildren, HttpContext.RequestAborted);
        return Ok(summaries);
    }

    [HttpGet("type/{sheetTypeId:int}")]
    public async Task<ActionResult<IReadOnlyList<SheetSummary>>> GetSheetSummariesByTypeAsync(int sheetTypeId)
    {
        var summaries = await _sheetDataAccessService.GetSheetSummariesByTypeAsync(sheetTypeId, HttpContext.RequestAborted);
        return Ok(summaries);
    }

    [HttpGet("phase/{phaseId:int}/type/{sheetTypeId:int}")]
    public async Task<ActionResult<SheetSummary>> GetSheetSummaryByPhaseAndTypeAsync(int phaseId, int sheetTypeId)
    {
        var summary = await _sheetDataAccessService.GetSheetSummaryByPhaseAndTypeAsync(phaseId, sheetTypeId, HttpContext.RequestAborted);
        return summary is null ? NotFound() : Ok(summary);
    }

    [HttpGet("phase/{phaseId:int}/type/{sheetTypeId:int}/latest")]
    public async Task<ActionResult<Sheet>> GetLatestPublishedSheetByPhaseAndTypeAsync(int phaseId, int sheetTypeId, [FromQuery] DateTime? publishedBy = null)
    {
        var sheet = await _sheetDataAccessService.GetLatestPublishedSheetByPhaseAndTypeAsync(phaseId, sheetTypeId, publishedBy, HttpContext.RequestAborted);
        return sheet is null ? NotFound() : Ok(sheet);
    }

    [HttpGet("spec-sheet/{specSheetId:int}")]
    public async Task<ActionResult<SheetLocation>> FindSheetAsync(int specSheetId)
    {
        var location = await _sheetDataAccessService.FindSheetAsync(specSheetId, HttpContext.RequestAborted);
        return location is null ? NotFound() : Ok(location);
    }

    [HttpGet("{id:int}/versions")]
    public async Task<ActionResult<IReadOnlyList<SheetVersion>>> GetSheetVersionsAsync(int id)
    {
        var versions = await _sheetDataAccessService.GetSheetVersionsAsync(id, HttpContext.RequestAborted);
        return Ok(versions);
    }
}
