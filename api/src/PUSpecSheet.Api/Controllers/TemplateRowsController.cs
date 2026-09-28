using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Row changes. Every action returns the whole updated template.</summary>
[ApiController]
[Route("api/template-rows")]
public sealed class TemplateRowsController(ITemplateRowService rows) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateRowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await rows.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<TableTemplateDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await rows.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
