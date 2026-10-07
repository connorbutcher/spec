using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Templates;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Section changes. Every action returns the whole updated template.</summary>
[ApiController]
[Tags(ApiTags.TemplateSections)]
[Route("api/template-sections")]
[Authorize(Policy = PermissionKeys.TemplatesManage)]
public sealed class TemplateSectionsController(ITemplateSectionService sections) : ControllerBase
{
    /// <summary>Add a section</summary>
    /// <remarks>
    /// Adds a section to the end of a template version's top level, or of a parent section's children.
    /// </remarks>
    /// <param name="request">The template version, optional parent section and name.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPost]
    public async Task<ActionResult<TableTemplateDto>> Create(
        CreateTemplateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sections.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Update a section</summary>
    /// <remarks>
    /// Renames a section and sets how many copies a sheet table may hold. The counts must satisfy
    /// minimum &lt;= starts with &lt;= maximum, and a null maximum means no limit. The header's counts are
    /// fixed at one and are ignored.
    /// </remarks>
    /// <param name="id">The template section's id.</param>
    /// <param name="request">The new name and copy counts.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateSectionRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sections.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Move a section</summary>
    /// <remarks>
    /// Moves the section to a position among its siblings.
    /// </remarks>
    /// <param name="id">The template section's id.</param>
    /// <param name="request">The 1-based position to move to. A position past the end moves it last.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<TableTemplateDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await sections.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Delete a section</summary>
    /// <remarks>
    /// Deletes the section with its rows, cells and sub-sections. The header can't be deleted.
    /// </remarks>
    /// <param name="id">The template section's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<TableTemplateDto>> Delete(int id, CancellationToken cancellationToken)
    {
        var result = await sections.DeleteAsync(id, cancellationToken);
        return Ok(result);
    }
}
