using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Cell changes. Every action returns the whole updated template.</summary>
[ApiController]
[Route("api/template-cells")]
public sealed class TemplateCellsController(ITemplateCellService cells) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateCellRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cells.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateCellRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cells.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await cells.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
