using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Abstractions.Operations;
using PUSpecSheet.Abstractions.Requests.SpecSheets;
using PUSpecSheet.Api.Common.Authorization;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

namespace PUSpecSheet.Api.Controllers;

[Route("specsheets")]
[Authorize]
public class SpecSheetController : ControllerBase
{
    private readonly ISpecSheetDataAccessService _specSheetDataAccessService;

    public SpecSheetController(ISpecSheetDataAccessService specSheetDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(specSheetDataAccessService, nameof(specSheetDataAccessService));

        _specSheetDataAccessService = specSheetDataAccessService;
    }

    [Obsolete("Use GET sheets/spec-sheet/{specSheetId} to find the sheet, then GET sheets/{id} or sheets/{id}/draft.")]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpecSheet>> GetSpecSheetByIdAsync([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var specSheet = await _specSheetDataAccessService.GetSpecSheetByIdAsync(id, HttpContext.RequestAborted);
        return Ok(specSheet);
    }

    [Obsolete("Use GET sheets/phase/{phaseId}/type/{sheetTypeId}/latest.")]
    [HttpGet("latest/published/type/{typeId:int}/phase/{phaseId:int}")]
    public async Task<ActionResult<SpecSheet?>> GetLatestPublishedSpecSheetByTypeAndPhaseAsync(
        [FromRoute] int typeId,
        [FromRoute] int phaseId,
        [FromQuery] DateTime? publishedByDateTime)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var specSheet = await _specSheetDataAccessService.GetLatestPublishedSpecSheetByTypeAndPhaseAsync(typeId, phaseId, publishedByDateTime, HttpContext.RequestAborted);
        return Ok(specSheet);
    }

    [Obsolete("Use GET sheets/phase/{phaseId}/all.")]
    [HttpGet("phase/{phaseId:int}/all")]
    public async Task<ActionResult<ICollection<SpecSheetSummary>>> GetAllSpecSheetsByPhaseAsync([FromRoute] int phaseId, [FromQuery] bool includeChildren = false)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var specSheets = await _specSheetDataAccessService.GetAllSpecSheetsByPhaseAsync(phaseId, includeChildren, HttpContext.RequestAborted);
        return Ok(specSheets);
    }

    [Obsolete("Use GET sheets/phase/{phaseId}/type/{sheetTypeId}.")]
    [HttpGet("phase/{phaseId:int}/type/{typeId:int}")]
    [HttpGet("phase/{phaseId:int}/type/{typeId:int}/published/{published:bool}")]
    public async Task<ActionResult<ICollection<SpecSheetSummary>>> GetAllSpecSheetsByPhaseAndTypeAsync([FromRoute] int phaseId, [FromRoute] int typeId, [FromRoute] bool? published)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var specSheets = await _specSheetDataAccessService.GetAllSpecSheetsByPhaseAndTypeAsync(phaseId, typeId, published, HttpContext.RequestAborted);
        return Ok(specSheets);
    }

    [Obsolete("Use GET sheets/type/{sheetTypeId}.")]
    [HttpGet("type/{typeId:int}")]
    public async Task<ActionResult<ICollection<SpecSheetSummary>>> GetAllSpecSheetsByTypeAsync([FromRoute] int typeId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var specSheets = await _specSheetDataAccessService.GetAllSpecSheetsByTypeAsync(typeId, HttpContext.RequestAborted);
        return Ok(specSheets);
    }

    [Obsolete("Use POST sheets/draft.")]
    [HttpPost("create")]
    public async Task<ActionResult<int>> CreateSpecSheetAsync([FromBody] CreateSpecSheetRequest createPUSpecSheetRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var specSheetId = await _specSheetDataAccessService.CreateSpecSheetAsync(createPUSpecSheetRequest, HttpContext.RequestAborted);
        return Ok(specSheetId);
    }

    [Obsolete("Use PATCH sheets/{id}/draft.")]
    [HttpPatch("update")]
    public async Task<ActionResult> UpdateSpecSheetAsync([FromBody] SpecSheetChanges specSheetChanges)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _specSheetDataAccessService.UpdateSpecSheetAsync(specSheetChanges, HttpContext.RequestAborted);
        return Ok();
    }

    [Obsolete("Use PATCH sheets/{id}/publish.")]
    [HttpPatch("publish")]
    public async Task<ActionResult> PublishSpecSheetAsync([FromBody] PublishSpecSheetRequest publishSpecSheetRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _specSheetDataAccessService.PublishSpecSheetAsync(publishSpecSheetRequest, HttpContext.RequestAborted);
        return Ok();
    }

    [Obsolete("Use DELETE sheets/{id}/draft.")]
    [HttpDelete("{id:int}")]
    [UserPermissionAuthorization("admin.view", "admin.sheets")]
    public async Task<ActionResult> DeleteSpecSheetAsync([FromRoute] int id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _specSheetDataAccessService.DeleteSpecSheetAsync(id, HttpContext.RequestAborted);
        return Ok();
    }
}
