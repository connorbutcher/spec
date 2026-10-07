using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Column block changes, for horizontal tables. Every action returns the whole updated template.</summary>
[ApiController]
[Tags(ApiTags.TemplateColumnBlocks)]
[Route("api/template-column-blocks")]
[Authorize(Policy = PermissionKeys.TemplatesManage)]
public sealed class TemplateColumnBlocksController(ITemplateColumnBlockService blocks) : ControllerBase
{
    /// <summary>Add a column block</summary>
    /// <remarks>
    /// Adds a column block to the right of a horizontal template version's other blocks. Each row of the version gets one cell in the new block to start with.
    /// </remarks>
    /// <param name="request">The template version and the block's name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    /// <response code="409">The template version is in use, or the template isn't horizontal.</response>
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await blocks.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Update a column block</summary>
    /// <remarks>
    /// Renames a column block and sets how many copies a sheet table may hold, and how many of its leading
    /// columns stay in view. The counts must satisfy minimum &lt;= starts with &lt;= maximum, and a null
    /// maximum means no limit.
    /// </remarks>
    /// <param name="id">The template column block's id.</param>
    /// <param name="request">The new name, copy counts and sticky column count.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateColumnBlockRequest request,
        CancellationToken cancellationToken)
    {
        var result = await blocks.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Move a column block</summary>
    /// <remarks>
    /// Moves the column block to a position among the version's blocks, counted from the left.
    /// </remarks>
    /// <param name="id">The template column block's id.</param>
    /// <param name="request">The 1-based position to move to. A position past the end moves it last.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<TableTemplateDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await blocks.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a column block</summary>
    /// <remarks>
    /// Deletes the column block with its cells in every row.
    /// </remarks>
    /// <param name="id">The template column block's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await blocks.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
