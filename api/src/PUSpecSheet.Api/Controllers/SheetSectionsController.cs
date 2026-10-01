using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Changes to one section copy on a sheet. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Route("api/sheet-sections")]
public sealed class SheetSectionsController(ISheetSectionService sections, ISheetRowService rows) : ControllerBase
{
    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<SheetDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await sections.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<SheetDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await sections.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/rows")]
    public async Task<ActionResult<SheetDto>> AddRow(int id, AddSheetRowRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.AddAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
