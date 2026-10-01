using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Column block changes, for horizontal tables. Every action returns the whole updated template.</summary>
[ApiController]
[Route("api/template-column-blocks")]
public sealed class TemplateColumnBlocksController(ITemplateColumnBlockService blocks) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await blocks.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await blocks.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<TableTemplateDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await blocks.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await blocks.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
