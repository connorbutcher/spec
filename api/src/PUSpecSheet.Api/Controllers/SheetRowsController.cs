using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Changes to one row on a sheet. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Route("api/sheet-rows")]
public sealed class SheetRowsController(ISheetRowService rows) : ControllerBase
{
    [HttpPost("{id:int}/lock")]
    public async Task<ActionResult<SheetDto>> Lock(int id, CancellationToken cancellationToken)
    {
        var result = await rows.LockAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:int}/values")]
    public async Task<ActionResult<SheetDto>> SaveValues(int id, SaveRowValuesRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.SaveValuesAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<SheetDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<SheetDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await rows.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}/draft")]
    public async Task<ActionResult<SheetDto>> Discard(int id, CancellationToken cancellationToken)
    {
        var result = await rows.DiscardAsync(id, cancellationToken);
        return Ok(result);
    }
}
