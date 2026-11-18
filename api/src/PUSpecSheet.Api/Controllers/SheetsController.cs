using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Abstractions.Requests.Sheets;
using PUSpecSheet.Api.Common.Authorization;
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

    [HttpGet("{id:int}/submissions")]
    public async Task<ActionResult<IReadOnlyList<SheetSubmission>>> GetSheetSubmissionsAsync(int id, [FromQuery] int? version = null)
    {
        var submissions = await _sheetDataAccessService.GetSheetSubmissionsAsync(id, version, HttpContext.RequestAborted);
        return submissions is null ? NotFound() : Ok(submissions);
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
    [UserPermissionAuthorization("admin.view", "admin.sheets")]
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

    #region By Spec Sheet Id

    // The app opens, edits and lists sheets by the id of a spec sheet (a published version's or a draft's), which these
    // endpoints take in place of the sheet's.

    [HttpGet("spec-sheet/{specSheetId:int}/sheet")]
    public async Task<ActionResult<Sheet>> GetSheetBySpecSheetAsync(int specSheetId)
    {
        var location = await _sheetDataAccessService.FindSheetAsync(specSheetId, HttpContext.RequestAborted);
        if (location is null)
        {
            return NotFound();
        }

        var sheet = location.IsDraft
            ? await _sheetDataAccessService.GetDraftSheetAsync(location.SheetId, HttpContext.RequestAborted)
            : await _sheetDataAccessService.GetPublishedSheetAsync(location.SheetId, location.VersionNumber, HttpContext.RequestAborted);
        return sheet is null ? NotFound() : Ok(sheet);
    }

    [HttpGet("spec-sheet/{specSheetId:int}/submissions")]
    public async Task<ActionResult<IReadOnlyList<SheetSubmission>>> GetSubmissionsBySpecSheetAsync(int specSheetId)
    {
        var location = await _sheetDataAccessService.FindSheetAsync(specSheetId, HttpContext.RequestAborted);
        if (location is null)
        {
            return NotFound();
        }

        var submissions = await _sheetDataAccessService.GetSheetSubmissionsAsync(location.SheetId, location.VersionNumber, HttpContext.RequestAborted);
        return submissions is null ? NotFound() : Ok(submissions);
    }

    [HttpPatch("spec-sheet/{specSheetId:int}/draft")]
    public async Task<ActionResult> ApplyChangesBySpecSheetAsync(int specSheetId, [FromBody] SheetChanges changes)
    {
        var location = await FindDraftAsync(specSheetId);
        if (location is null)
        {
            return NotFound();
        }

        await _sheetDataAccessService.ApplyChangesAsync(location.SheetId, changes, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPatch("spec-sheet/{specSheetId:int}/publish")]
    public async Task<ActionResult> PublishBySpecSheetAsync(int specSheetId, [FromBody] PublishSheetRequest request)
    {
        var location = await FindDraftAsync(specSheetId);
        if (location is null)
        {
            return NotFound();
        }

        await _sheetDataAccessService.PublishAsync(location.SheetId, request, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpDelete("spec-sheet/{specSheetId:int}/draft")]
    [UserPermissionAuthorization("admin.view", "admin.sheets")]
    public async Task<ActionResult> DeleteDraftBySpecSheetAsync(int specSheetId)
    {
        var location = await FindDraftAsync(specSheetId);
        if (location is null)
        {
            return NotFound();
        }

        await _sheetDataAccessService.DeleteDraftAsync(location.SheetId, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPatch("spec-sheet/{specSheetId:int}/submission/decision")]
    public async Task<ActionResult> DecideSubmissionBySpecSheetAsync(int specSheetId, [FromBody] DecideSubmissionRequest request)
    {
        var location = await FindDraftAsync(specSheetId);
        if (location is null)
        {
            return NotFound();
        }

        await _sheetDataAccessService.DecideSubmissionAsync(location.SheetId, request, HttpContext.RequestAborted);
        return NoContent();
    }

    [HttpPatch("spec-sheet/{specSheetId:int}/submission/approvers")]
    public async Task<ActionResult> ReassignSubmissionApproversBySpecSheetAsync(int specSheetId, [FromBody] ReassignSubmissionApproversRequest request)
    {
        var location = await FindDraftAsync(specSheetId);
        if (location is null)
        {
            return NotFound();
        }

        await _sheetDataAccessService.ReassignSubmissionApproversAsync(location.SheetId, request, HttpContext.RequestAborted);
        return NoContent();
    }

    // Every published version of the sheets of the phase and type, then the draft's id where there is one.
    [HttpGet("phase/{phaseId:int}/type/{sheetTypeId:int}/versions")]
    public async Task<ActionResult<IReadOnlyList<SheetVersionSummary>>> GetVersionSummariesByPhaseAndTypeAsync(int phaseId, int sheetTypeId)
    {
        return Ok(await _sheetDataAccessService.GetPublishedVersionSummariesAsync(sheetTypeId, phaseId, HttpContext.RequestAborted));
    }

    [HttpGet("type/{sheetTypeId:int}/versions")]
    public async Task<ActionResult<IReadOnlyList<SheetVersionSummary>>> GetVersionSummariesByTypeAsync(int sheetTypeId)
    {
        return Ok(await _sheetDataAccessService.GetPublishedVersionSummariesAsync(sheetTypeId, null, HttpContext.RequestAborted));
    }

    private async Task<SheetLocation?> FindDraftAsync(int specSheetId)
    {
        var location = await _sheetDataAccessService.FindSheetAsync(specSheetId, HttpContext.RequestAborted);
        return location is { IsDraft: true } ? location : null;
    }

    #endregion By Spec Sheet Id
}
