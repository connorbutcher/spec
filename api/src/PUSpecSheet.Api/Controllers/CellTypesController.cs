using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Contracts.CellTypes;

namespace PUSpecSheet.Api.Controllers;

[ApiController]
[Route("api/cell-types")]
public sealed class CellTypesController(ICellTypeService cellTypes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CellTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await cellTypes.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<CellTypeDto>> Create(SaveCellTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await cellTypes.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<CellTypeDto>> Update(
        int id,
        SaveCellTypeRequest request,
        CancellationToken cancellationToken)
    {
        var result = await cellTypes.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await cellTypes.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
