using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Section changes. Every action returns the whole updated template.</summary>
[ApiController]
[Route("api/template-sections")]
public sealed class TemplateSectionsController(ITemplateSectionService sections) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sections.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sections.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<TableTemplateDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await sections.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await sections.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
