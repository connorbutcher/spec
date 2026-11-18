// using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Abstractions.Requests.Types.Phase;
// using PUSpecSheet.Api.Common.Authorization;
using PUSpecSheet.Api.DAL.DataAccess.Types;

namespace PUSpecSheet.Api.Controllers;

[Route("specsheetphases")]
// [Authorize]
public class SpecSheetPhasesController : ControllerBase
{
    private readonly ITypeDataAccessService _typeDataAccessService;

    public SpecSheetPhasesController(ITypeDataAccessService typeDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(typeDataAccessService, nameof(typeDataAccessService));

        _typeDataAccessService = typeDataAccessService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<SpecSheetPhase>>> GetAllSpecSheetPhasesAsync()
    {
        var specSheetGroups = await _typeDataAccessService.GetAllSpecSheetPhasesAsync(HttpContext.RequestAborted);
        return Ok(specSheetGroups);
    }

    [HttpPost("create")]
    // [UserPermissionAuthorization("admin.view", "admin.phases")]
    public async Task<ActionResult> CreateSpecSheetPhaseAsync([FromBody] CreateSpecSheetPhaseRequest createSpecSheetPhaseRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _typeDataAccessService.CreateSpecSheetPhaseAsync(createSpecSheetPhaseRequest, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPatch("update")]
    // [UserPermissionAuthorization("admin.view", "admin.phases")]
    public async Task<ActionResult> UpdateSpecSheetPhaseAsync([FromBody] UpdateSpecSheetPhaseRequest updateSpecSheetPhaseRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _typeDataAccessService.UpdateSpecSheetPhaseAsync(updateSpecSheetPhaseRequest, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPatch("update/many")]
    // [UserPermissionAuthorization("admin.view", "admin.phases")]
    public async Task<ActionResult> UpdateManySpecSheetPhasesAsync([FromBody] List<UpdateSpecSheetPhaseRequest> updateRequests)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _typeDataAccessService.UpdateManySpecSheetPhasesAsync(updateRequests, HttpContext.RequestAborted);
        return Ok();
    }
}
