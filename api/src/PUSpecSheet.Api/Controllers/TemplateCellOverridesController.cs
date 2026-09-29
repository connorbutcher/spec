using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

/// <summary>What a cell changes from its cell type's default configuration and style. Returns the whole updated template.</summary>
[ApiController]
[Route("api/template-cells/{id:int}/overrides")]
public sealed class TemplateCellOverridesController(ITemplateCellOverrideService overrides) : ControllerBase
{
    [HttpPut]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateCellOverridesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await overrides.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
