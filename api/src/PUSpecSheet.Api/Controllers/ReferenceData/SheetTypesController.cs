using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.SheetTypes;
using PUSpecSheet.Abstractions.Users;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.DataAccess.SheetTypes;

namespace PUSpecSheet.Api.Controllers.ReferenceData;

[ApiController]
[Tags(ApiTags.SheetTypes)]
[Route("api/sheet-types")]
public sealed class SheetTypesController : ControllerBase
{
    private readonly ISheetTypesService _sheetTypes;

    public SheetTypesController(ISheetTypesService sheetTypes)
    {
        _sheetTypes = sheetTypes;
    }

    /// <summary>List sheet types</summary>
    /// <remarks>
    /// The kinds of sheet a phase can have, such as Specification, PFKs or Parts, in display order.
    /// </remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">All sheet types.</response>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SheetType>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await _sheetTypes.GetAllAsync(cancellationToken);
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
    public async Task<ActionResult<SheetType>> Create(CreateSheetTypeRequest request, CancellationToken cancellationToken)
    {
        var result = await _sheetTypes.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Add an identifier to a sheet type</summary>
    /// <remarks>
    /// Adds a value that tells one sheet of the type from another within a phase, such as a serial number,
    /// to the end of the type's list. Every new sheet of the type asks for it; sheets already made keep the
    /// values they have. Its <c>key</c>, made from the name, names it in the published API's filters and
    /// never changes. Needs the <c>sheetTypes.manage</c> permission.
    /// </remarks>
    /// <param name="id">The sheet type's id.</param>
    /// <param name="request">The identifier's name, kind, options and whether it is required.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet type with its identifiers.</response>
    /// <response code="400">The name is blank, an option identifier has no options, or the type already has the most identifiers allowed.</response>
    /// <response code="404">The sheet type doesn't exist.</response>
    /// <response code="409">The type already has an identifier with this name.</response>
    [HttpPost("{id:int}/identifiers")]
    [Authorize(Policy = PermissionKeys.SheetTypesManage)]
    public async Task<ActionResult<SheetType>> AddIdentifier(int id, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken)
    {
        var result = await _sheetTypes.AddIdentifierAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Change an identifier of a sheet type</summary>
    /// <remarks>
    /// Changes an identifier's name, options or whether it is required; the values sheets already have are
    /// kept. Its kind can only change while no sheet has a value for it. Needs the <c>sheetTypes.manage</c> permission.
    /// </remarks>
    /// <param name="id">The sheet type's id.</param>
    /// <param name="identifierId">The identifier's id.</param>
    /// <param name="request">The identifier's name, kind, options and whether it is required.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet type with its identifiers.</response>
    /// <response code="400">The name is blank or an option identifier has no options.</response>
    /// <response code="404">The sheet type or identifier doesn't exist.</response>
    /// <response code="409">Another identifier has this name, or the kind changed while sheets have values for it.</response>
    [HttpPut("{id:int}/identifiers/{identifierId:int}")]
    [Authorize(Policy = PermissionKeys.SheetTypesManage)]
    public async Task<ActionResult<SheetType>> UpdateIdentifier(int id, int identifierId, SaveSheetTypeIdentifierRequest request, CancellationToken cancellationToken)
    {
        var result = await _sheetTypes.UpdateIdentifierAsync(id, identifierId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Remove an identifier from a sheet type</summary>
    /// <remarks>
    /// Removes an identifier that no sheet has a value for. Needs the <c>sheetTypes.manage</c> permission.
    /// </remarks>
    /// <param name="id">The sheet type's id.</param>
    /// <param name="identifierId">The identifier's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The sheet type with its identifiers.</response>
    /// <response code="404">The sheet type or identifier doesn't exist.</response>
    /// <response code="409">Sheets have a value for the identifier.</response>
    [HttpDelete("{id:int}/identifiers/{identifierId:int}")]
    [Authorize(Policy = PermissionKeys.SheetTypesManage)]
    public async Task<ActionResult<SheetType>> DeleteIdentifier(int id, int identifierId, CancellationToken cancellationToken)
    {
        var result = await _sheetTypes.DeleteIdentifierAsync(id, identifierId, cancellationToken);
        return Ok(result);
    }
}
