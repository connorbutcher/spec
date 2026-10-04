using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Changes to the column block copies on a horizontal sheet table. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Route("api")]
public sealed class SheetColumnBlocksController(ISheetColumnBlockService columnBlocks) : ControllerBase
{
    [HttpPost("sheet-tables/{tableId:int}/column-blocks")]
    public async Task<ActionResult<SheetDto>> Add(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken)
    {
        var result = await columnBlocks.AddAsync(tableId, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("sheet-column-blocks/{id:int}/move")]
    public async Task<ActionResult<SheetDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await columnBlocks.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("sheet-column-blocks/{id:int}")]
    public async Task<ActionResult<SheetDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await columnBlocks.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }
}
