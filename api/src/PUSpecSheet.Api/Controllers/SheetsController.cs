// using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Requests.Sheets;
// using PUSpecSheet.Api.Common.Authorization;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Sheets in the shape of the revision schema. Ids here are sheet ids and the ids of the sheet's items.
/// </summary>
[Route("sheets")]
// [Authorize]
public class SheetsController : ControllerBase
{
    private readonly ISheetDataAccessService _sheetDataAccessService;

    public SheetsController(ISheetDataAccessService sheetDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(sheetDataAccessService, nameof(sheetDataAccessService));

        _sheetDataAccessService = sheetDataAccessService;
    }

    [HttpGet("{id:int}/detail")]
    public async Task<ActionResult<SheetDetail>> GetPublishedSheetDetailAsync(int id, [FromQuery] int? version = null)
    {
        var detail = await _sheetDataAccessService.GetPublishedSheetDetailAsync(id, version, HttpContext.RequestAborted);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpGet("{id:int}/draft/detail")]
    public async Task<ActionResult<SheetDetail>> GetDraftSheetDetailAsync(int id)
    {
        var detail = await _sheetDataAccessService.GetDraftSheetDetailAsync(id, HttpContext.RequestAborted);
        return detail is null ? NotFound() : Ok(detail);
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

    [HttpGet("phase/{phaseId:int}/type/{sheetTypeId:int}/latest")]
    public async Task<ActionResult<Sheet>> GetLatestPublishedSheetByPhaseAndTypeAsync(int phaseId, int sheetTypeId, [FromQuery] DateTime? publishedBy = null)
    {
        var sheet = await _sheetDataAccessService.GetLatestPublishedSheetByPhaseAndTypeAsync(phaseId, sheetTypeId, publishedBy, HttpContext.RequestAborted);
        return sheet is null ? NotFound() : Ok(sheet);
    }

    [HttpGet("phase/{phaseId:int}/type/{sheetTypeId:int}/cells")]
    public async Task<ActionResult<SheetCells>> GetLatestPublishedCellsByPhaseAndTypeAsync(int phaseId, int sheetTypeId, [FromQuery] Guid[] referenceIds, [FromQuery] DateTime? publishedBy = null)
    {
        var cells = await _sheetDataAccessService.GetLatestPublishedCellsByPhaseAndTypeAsync(phaseId, sheetTypeId, referenceIds, publishedBy, HttpContext.RequestAborted);
        return cells is null ? NotFound() : Ok(cells);
    }

    [HttpGet("{id:int}/cells")]
    public async Task<ActionResult<SheetCells>> GetPublishedCellsAsync(int id, [FromQuery] Guid[] referenceIds, [FromQuery] int? version = null)
    {
        var cells = await _sheetDataAccessService.GetPublishedCellsAsync(id, version, referenceIds, HttpContext.RequestAborted);
        return cells is null ? NotFound() : Ok(cells);
    }

    [HttpGet("tables/unique")]
    public async Task<ActionResult<IReadOnlyList<SheetTable>>> GetUniqueTablesAsync()
    {
        var tables = await _sheetDataAccessService.GetUniqueTablesAsync(HttpContext.RequestAborted);
        return Ok(tables);
    }

    [HttpGet("{id:int}/versions")]
    public async Task<ActionResult<IReadOnlyList<SheetVersion>>> GetSheetVersionsAsync(int id)
    {
        var versions = await _sheetDataAccessService.GetSheetVersionsAsync(id, HttpContext.RequestAborted);
        return Ok(versions);
    }

    [HttpGet("phase/{phaseId:int}/type/{sheetTypeId:int}/list")]
    public async Task<ActionResult<IReadOnlyList<SheetListItem>>> GetSheetListAsync(int phaseId, int sheetTypeId)
    {
        var items = await _sheetDataAccessService.GetSheetListAsync(phaseId, sheetTypeId, HttpContext.RequestAborted);
        return Ok(items);
    }

    [HttpGet("type/{sheetTypeId:int}/list")]
    public async Task<ActionResult<IReadOnlyList<SheetListItem>>> GetPublishedSheetListAsync(int sheetTypeId)
    {
        var items = await _sheetDataAccessService.GetPublishedSheetListAsync(sheetTypeId, HttpContext.RequestAborted);
        return Ok(items);
    }

    [HttpPost("draft")]
    public async Task<ActionResult<SheetLocation>> CreateDraftAsync([FromBody] CreateSheetRequest request)
    {
        var location = await _sheetDataAccessService.CreateDraftAsync(request, HttpContext.RequestAborted);
        return Ok(location);
    }

    [HttpPatch("{id:int}/draft")]
    public async Task<ActionResult> ApplyChangesAsync(int id, [FromBody] SheetChanges changes)
    {
        await _sheetDataAccessService.ApplyChangesAsync(id, changes, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPatch("{id:int}/publish")]
    public async Task<ActionResult> PublishAsync(int id, [FromBody] PublishSheetRequest request)
    {
        await _sheetDataAccessService.PublishAsync(id, request, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpDelete("{id:int}/draft")]
    // [UserPermissionAuthorization("admin.view", "admin.sheets")]
    public async Task<ActionResult> DeleteDraftAsync(int id)
    {
        await _sheetDataAccessService.DeleteDraftAsync(id, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPatch("{id:int}/submission/decision")]
    public async Task<ActionResult> DecideSubmissionAsync(int id, [FromBody] DecideSubmissionRequest request)
    {
        await _sheetDataAccessService.DecideSubmissionAsync(id, request, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPatch("{id:int}/submission/approvers")]
    public async Task<ActionResult> ReassignSubmissionApproversAsync(int id, [FromBody] ReassignSubmissionApproversRequest request)
    {
        await _sheetDataAccessService.ReassignSubmissionApproversAsync(id, request, HttpContext.RequestAborted);
        return NoContent();
    }
}
