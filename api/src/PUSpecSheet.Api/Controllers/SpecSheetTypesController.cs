using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Abstractions.Requests.Types.Type;
using PUSpecSheet.Api.Common.Authorization;
using PUSpecSheet.Api.DAL.DataAccess.Types;

namespace PUSpecSheet.Api.Controllers;

[Route("specsheettypes")]
[Authorize]
public class SpecSheetTypesController : ControllerBase
{
    private readonly ITypeDataAccessService _typeDataAccessService;

    public SpecSheetTypesController(ITypeDataAccessService typeDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(typeDataAccessService, nameof(typeDataAccessService));

        _typeDataAccessService = typeDataAccessService;
    }

    [HttpGet("all")]
    public async Task<ActionResult<ICollection<SpecSheetType>>> GetAllSpecSheetTypesAsync()
    {
        var specSheetTypes = await _typeDataAccessService.GetAllSpecSheetTypesAsync(HttpContext.RequestAborted);
        return Ok(specSheetTypes);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<SpecSheetType>> GetSpecSheetTypeByIdAsync([FromRoute] int id)
    {
        var specSheetType = await _typeDataAccessService.GetSpecSheetTypeByIdAsync(id, HttpContext.RequestAborted);
        return Ok(specSheetType);
    }

    [HttpPost("create")]
    [UserPermissionAuthorization("admin.view", "admin.types")]
    public async Task<ActionResult> CreateSpecSheetTypeAsync([FromBody] CreateSpecSheetTypeRequest createSpecSheetTypeRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _typeDataAccessService.CreateSpecSheetTypeAsync(createSpecSheetTypeRequest, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpPatch("update")]
    [UserPermissionAuthorization("admin.view", "admin.types")]
    public async Task<ActionResult> UpdateSpecSheetTypeAsync([FromBody] UpdateSpecSheetTypeRequest updateSpecSheetTypeRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _typeDataAccessService.UpdateSpecSheetTypeAsync(updateSpecSheetTypeRequest, HttpContext.RequestAborted);
        return Ok();
    }
}
