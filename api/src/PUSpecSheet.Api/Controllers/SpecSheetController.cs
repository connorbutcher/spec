using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.SpecSheets;
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
