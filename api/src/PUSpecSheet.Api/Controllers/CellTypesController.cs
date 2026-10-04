using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Contracts.CellTypes;

namespace PUSpecSheet.Api.Controllers;

[ApiController]
[Tags(ApiTags.CellTypes)]
[Route("api/cell-types")]
public sealed class CellTypesController(ICellTypeService cellTypes) : ControllerBase
{
    /// <summary>List cell types</summary>
    /// <remarks>
    /// Every cell type in display order, each with its default configuration, style, dropdown options and how many template cells use it.
    /// </remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">All cell types.</response>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CellTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await cellTypes.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Create a cell type</summary>
    /// <remarks>
    /// Adds a cell type to the end of the list. <c>configuration</c> must be for the same kind as <c>kind</c>;
    /// one for a different kind, or none, starts the kind with nothing set. <c>options</c> is the full list of
    /// dropdown choices, in order, and is ignored for other kinds.
    /// </remarks>
    /// <param name="request">The cell type's name, kind, defaults and dropdown options.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The new cell type.</response>
    /// <response code="409">Another cell type already has this name.</response>
    [HttpPost]
    public async Task<ActionResult<CellTypeDto>> Create(SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await cellTypes.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Replace a cell type</summary>
    /// <remarks>
    /// Replaces the cell type's name, kind, defaults and dropdown options. Template cells that use it pick up the new defaults wherever they don't override them.
    /// </remarks>
    /// <param name="id">The cell type's id.</param>
    /// <param name="request">The cell type's new name, kind, defaults and dropdown options.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The updated cell type.</response>
    /// <response code="404">No cell type has this id.</response>
    /// <response code="409">Another cell type has this name, or the change doesn't fit the cells that use it.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<CellTypeDto>> Update(
        int id,
        SaveCellTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cellTypes.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a cell type</summary>
    /// <remarks>
    /// Deletes a cell type that no template cell uses. <c>usageCount</c> on the cell type says whether it is in use.
    /// </remarks>
    /// <param name="id">The cell type's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="204">The cell type was deleted.</response>
    /// <response code="404">No cell type has this id.</response>
    /// <response code="409">Template cells still use this cell type.</response>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await cellTypes.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
