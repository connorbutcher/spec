using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Changes to one section copy on a sheet. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Tags(ApiTags.SheetSections)]
[Route("api/sheet-sections")]
[Authorize(Policy = PermissionKeys.SheetsEdit)]
public sealed class SheetSectionsController(ISheetSectionService sections, ISheetRowService rows) : ControllerBase
{
    /// <summary>Move a section</summary>
    /// <remarks>
    /// Moves the section copy to a position among its siblings.
    /// </remarks>
    /// <param name="id">The sheet section's id.</param>
    /// <param name="request">The 1-based position to move to. A position past the end moves it last.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<SheetDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await sections.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Remove a section</summary>
    /// <remarks>
    /// Removes the section copy and everything in it. The header can't be removed, and a section can't go below the fewest copies its template allows.
    /// </remarks>
    /// <param name="id">The sheet section's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="409">Someone else is editing inside the section, or it is at the template's minimum.</response>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<SheetDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await sections.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Add a row to a section</summary>
    /// <remarks>
    /// Adds another row built from one of the template rows of the section's template section. Not allowed in the header.
    /// </remarks>
    /// <param name="id">The sheet section's id.</param>
    /// <param name="request">The template row to build the row from.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpPost("{id:int}/rows")]
    public async Task<ActionResult<SheetDto>> AddRow(int id, AddSheetRowRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.AddAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
