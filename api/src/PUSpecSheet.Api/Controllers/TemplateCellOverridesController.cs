using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>What a cell changes from its cell type's default configuration and style. Returns the whole updated template.</summary>
[ApiController]
[Tags(ApiTags.TemplateCells)]
[Route("api/template-cells/{id:int}/overrides")]
[Authorize(Policy = PermissionKeys.TemplatesManage)]
public sealed class TemplateCellOverridesController(ITemplateCellOverrideService overrides) : ControllerBase
{
    /// <summary>Set a cell's overrides</summary>
    /// <remarks>
    /// Replaces what the cell changes from its cell type's default configuration and style. Null, or a
    /// value that sets nothing, goes back to the defaults. <c>configurationOverride</c> must be for the cell
    /// type's kind.
    /// </remarks>
    /// <param name="id">The template cell's id.</param>
    /// <param name="request">The configuration and style overrides.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    /// <response code="400">The configuration override is for a different kind than the cell's type.</response>
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
