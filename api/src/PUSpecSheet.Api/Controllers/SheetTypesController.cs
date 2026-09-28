using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.SheetTypes;
using PUSpecSheet.Contracts.SheetTypes;

namespace PUSpecSheet.Api.Controllers;

[ApiController]
[Route("api/sheet-types")]
public sealed class SheetTypesController(ISheetTypeService sheetTypes) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SheetTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await sheetTypes.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
