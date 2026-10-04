using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.SheetTypes;
using PUSpecSheet.Contracts.SheetTypes;

namespace PUSpecSheet.Api.Controllers;

[ApiController]
[Tags(ApiTags.SheetTypes)]
[Route("api/sheet-types")]
public sealed class SheetTypesController(ISheetTypeService sheetTypes) : ControllerBase
{
    /// <summary>List sheet types</summary>
    /// <remarks>
    /// The kinds of sheet a phase can have, such as Specification, PFKs or Parts, in display order.
    /// </remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">All sheet types.</response>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SheetTypeDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await sheetTypes.GetAllAsync(cancellationToken);
        return Ok(result);
    }
}
