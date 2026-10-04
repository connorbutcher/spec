using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Finds any sheet, table, section, row or cell from its public identifier, which never changes between versions.</summary>
[ApiController]
[Tags(ApiTags.SheetItems)]
[Route("api/sheet-items")]
public sealed class SheetItemsController(ISheetItemLocator locator) : ControllerBase
{
    /// <summary>Find a sheet item</summary>
    /// <remarks>
    /// Says what a public identifier belongs to (a sheet, table, section, column block, row or cell) and
    /// where it lives. The identifier is the same in every version of the sheet. Parent ids are null where
    /// they don't apply, and <c>isDeleted</c> says whether the latest published revision removes the item.
    /// </remarks>
    /// <param name="publicId">The item's public identifier.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">What the item is and where it lives.</response>
    /// <response code="404">Nothing has this identifier.</response>
    [HttpGet("{publicId:guid}")]
    public async Task<ActionResult<SheetItemReferenceDto>> Find(Guid publicId, CancellationToken cancellationToken)
    {
        var result = await locator.FindAsync(publicId, cancellationToken);
        return Ok(result);
    }
}
