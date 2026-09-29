using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

[ApiController]
[Route("api/table-templates")]
public sealed class TableTemplatesController(ITableTemplateService templates) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TableTemplateSummaryDto>>> GetAll(
        [FromQuery] int? sheetTypeId,
        CancellationToken cancellationToken)
    {
        var result = await templates.GetSummariesAsync(sheetTypeId, cancellationToken);
        return Ok(result);
    }

    /// <summary>The template at <paramref name="version"/>, or at its latest version when omitted.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Get(
        int id,
        [FromQuery] int? version,
        CancellationToken cancellationToken)
    {
        var result = await templates.GetAsync(id, version, cancellationToken);
        return Ok(result);
    }

    /// <summary>Copies the latest version into a new editable version, for when the latest is in use.</summary>
    [HttpPost("{id:int}/versions")]
    public async Task<ActionResult<TableTemplateDto>> CreateVersion(int id, CancellationToken cancellationToken)
    {
        var result = await templates.CreateVersionAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTableTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await templates.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTableTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await templates.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await templates.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
