using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Changes to one table on a sheet. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Route("api/sheet-tables")]
public sealed class SheetTablesController(ISheetTableService tables, ISheetSectionService sections) : ControllerBase
{
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SheetDto>> SetTitle(int id, UpdateSheetTableRequest request, CancellationToken cancellationToken)
    {
        var result = await tables.SetTitleAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<SheetDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await tables.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<SheetDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await tables.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost("{id:int}/sections")]
    public async Task<ActionResult<SheetDto>> AddSection(int id, AddSheetSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await sections.AddAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
