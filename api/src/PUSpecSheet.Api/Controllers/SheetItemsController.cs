using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Finds any sheet, table, section, row or cell from its public identifier, which never changes between versions.</summary>
[ApiController]
[Route("api/sheet-items")]
public sealed class SheetItemsController(ISheetItemLocator locator) : ControllerBase
{
    [HttpGet("{publicId:guid}")]
    public async Task<ActionResult<SheetItemReferenceDto>> Find(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await locator.FindAsync(publicId, cancellationToken);
        return Ok(result);
    }
}
