using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Contracts.Templates;

namespace PUSpecSheet.Api.Controllers;

/// <summary>The name other applications look a template cell up by. Returns the whole updated template.</summary>
[ApiController]
[Tags(ApiTags.TemplateCells)]
[Route("api/template-cells/{id:int}/lookup-key")]
public sealed class TemplateCellLookupKeysController(ITemplateCellLookupKeyService lookupKeys) : ControllerBase
{
    /// <summary>Set a cell's lookup key</summary>
    /// <remarks>
    /// Names the cell so other applications can find it by the value typed into it, through
    /// <c>GET /api/published/lookup</c>: a part number cell given the key <c>partNumber</c> is found by
    /// <c>key=partNumber&amp;value=P-1003</c>. Null or blank removes the key.
    ///
    /// Only a text cell can have a key. A key starts with a letter and holds only letters, digits, dots,
    /// hyphens and underscores, up to 50 characters. Several cells can share a key. Unlike a cell's other
    /// settings this can change on a template version that sheets already use, and takes effect on those
    /// sheets at once.
    /// </remarks>
    /// <param name="id">The template cell's id.</param>
    /// <param name="request">The key, or null to remove it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The whole updated template.</response>
    /// <response code="400">The key isn't a valid name, or the cell isn't a text cell.</response>
    [HttpPut]
    public async Task<ActionResult<TableTemplateDto>> Update(
        int id,
        UpdateTemplateCellLookupKeyRequest request,
        CancellationToken cancellationToken)
    {
        var result = await lookupKeys.UpdateAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
