using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Cell changes. Every action returns the whole updated template.</summary>
[ApiController]
[Tags(ApiTags.TemplateCells)]
[Route("api/template-cells")]
public sealed class TemplateCellsController(ITemplateCellService cells) : ControllerBase
{
    /// <summary>Add a cell</summary>
    /// <remarks>
    /// Adds a cell to a row, among the row's own cells or, with <c>templateColumnBlockId</c>, among that
    /// column block's cells in the row. With <c>column</c> the cell goes in at that column and the cells from
    /// there on move one column right; without it the cell goes after the last one. Without a
    /// <c>cellTypeId</c> the cell uses the first Text cell type.
    /// </remarks>
    /// <param name="request">The row and, optionally, the cell type, column and column block.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateCellRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cells.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Update a cell</summary>
    /// <remarks>
    /// Sets the cell's type, column, spans, caption and whether a value is required before publishing.
    /// </remarks>
    /// <param name="id">The template cell's id.</param>
    /// <param name="request">The cell's new type, position, spans and caption.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    /// <response code="409">The template version is in use, or the cell would overlap another.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateCellRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cells.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a cell</summary>
    /// <remarks>
    /// Deletes the cell. The cells to its right keep their columns.
    /// </remarks>
    /// <param name="id">The template cell's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await cells.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
