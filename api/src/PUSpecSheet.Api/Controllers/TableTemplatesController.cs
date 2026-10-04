using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

[ApiController]
[Tags(ApiTags.TableTemplates)]
[Route("api/table-templates")]
public sealed class TableTemplatesController(ITableTemplateService templates) : ControllerBase
{
    /// <summary>List table templates</summary>
    /// <remarks>
    /// A summary of every table template in display order, optionally only those of one sheet type.
    /// </remarks>
    /// <param name="sheetTypeId">Only list this sheet type's templates.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The template summaries.</response>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TableTemplateSummaryDto>>> GetAll(
        [FromQuery] int? sheetTypeId,
        CancellationToken cancellationToken)
    {
        var result = await templates.GetSummariesAsync(sheetTypeId, cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a table template</summary>
    /// <remarks>
    /// The whole template, with its sections, rows, cells and column blocks, at a version or at its latest version.
    /// </remarks>
    /// <param name="id">The table template's id.</param>
    /// <param name="version" example="2">The version number to show. Leave out for the latest.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The template at that version.</response>
    /// <response code="404">The template or version doesn't exist.</response>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Get(
        int id,
        [FromQuery] int? version,
        CancellationToken cancellationToken)
    {
        var result = await templates.GetAsync(id, version, cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a new template version</summary>
    /// <remarks>
    /// Copies the latest version into a new editable version. Use it when the latest version is in use by a sheet and can no longer be edited. Existing sheet tables keep the version they were built from.
    /// </remarks>
    /// <param name="id">The table template's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The template at its new version.</response>
    [HttpPost("{id:int}/versions")]
    public async Task<ActionResult<TableTemplateDto>> CreateVersion(int id, CancellationToken cancellationToken)
    {
        var result = await templates.CreateVersionAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a table template</summary>
    /// <remarks>
    /// Adds a table template to the end of a sheet type's list.
    /// </remarks>
    /// <param name="request">The sheet type, name and orientation.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="201">The new template. <c>Location</c> is its address.</response>
    /// <response code="409">The sheet type already has a template with this name.</response>
    [HttpPost]
    [ProducesResponseType<TableTemplateDto>(StatusCodes.Status201Created)]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTableTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await templates.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    /// <summary>Update a table template</summary>
    /// <remarks>
    /// Sets the template's name, orientation and how many leading columns stay in view when a sheet table scrolls sideways.
    /// </remarks>
    /// <param name="id">The table template's id.</param>
    /// <param name="request">The new name, orientation and sticky column count.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The updated template.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTableTemplateRequest request,
        CancellationToken cancellationToken)
    {
        var result = await templates.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a table template</summary>
    /// <remarks>
    /// Deletes a template that no sheet uses.
    /// </remarks>
    /// <param name="id">The table template's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="204">The template was deleted.</response>
    /// <response code="409">A sheet has a table built from this template.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await templates.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
