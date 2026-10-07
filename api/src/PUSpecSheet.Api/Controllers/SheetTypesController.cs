using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.SheetTypes;
using PUSpecSheet.Contracts.SheetTypes;
using PUSpecSheet.Domain.Users;

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

    /// <summary>Create a sheet type</summary>
    /// <remarks>
    /// Adds a sheet type to the end of the list. No phase has it until it is made available to one with
    /// <c>PUT /api/phases/{id}/sheet-types</c>. Needs the <c>sheetTypes.manage</c> permission.
    /// </remarks>
    /// <param name="request">The sheet type's name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The new sheet type.</response>
    /// <response code="403">The current user doesn't have the <c>sheetTypes.manage</c> permission.</response>
    /// <response code="409">Another sheet type already has this name.</response>
    [HttpPost]
    [Authorize(Policy = PermissionKeys.SheetTypesManage)]
    public async Task<ActionResult<SheetTypeDto>> Create(CreateSheetTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await sheetTypes.CreateAsync(request, cancellationToken);
        return Ok(result);
    }
}
