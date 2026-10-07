using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Row changes. Every action returns the whole updated template.</summary>
[ApiController]
[Tags(ApiTags.TemplateRows)]
[Route("api/template-rows")]
[Authorize(Policy = PermissionKeys.TemplatesManage)]
public sealed class TemplateRowsController(ITemplateRowService rows) : ControllerBase
{
    /// <summary>Add a row</summary>
    /// <remarks>
    /// Adds a row to a section, at a 1-based position or at the end. The new row copies the columns of <c>copyFromRowId</c>, or of the section's last row when that is null.
    /// </remarks>
    /// <param name="request">The section, position and the row to copy columns from.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateRowRequest request,
        CancellationToken cancellationToken)
    {
        var result = await rows.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Move a row</summary>
    /// <remarks>
    /// Moves the row to a position among its section's rows.
    /// </remarks>
    /// <param name="id">The template row's id.</param>
    /// <param name="request">The 1-based position to move to. A position past the end moves it last.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<TableTemplateDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await rows.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a row</summary>
    /// <remarks>
    /// Deletes the row and its cells.
    /// </remarks>
    /// <param name="id">The template row's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await rows.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
