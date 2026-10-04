using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Contracts.Phases;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Read-only phase endpoints. Adding, editing and deleting phases will come with the admin section.</summary>
[ApiController]
[Tags(ApiTags.Phases)]
[Route("api/phases")]
public sealed class PhasesController(IPhaseService phases) : ControllerBase
{
    /// <summary>List phases</summary>
    /// <remarks>
    /// Every phase as a flat list in display order. Build the tree from <c>parentPhaseId</c>. <c>sheetTypeIds</c> says which sheets a phase can have.
    /// </remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">All phases.</response>
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PhaseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await phases.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    /// <summary>Get a phase</summary>
    /// <param name="id">The phase's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The phase.</response>
    /// <response code="404">No phase has this id.</response>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PhaseDto>> Get(int id, CancellationToken cancellationToken)
    {
        var result = await phases.GetAsync(id, cancellationToken);
        return Ok(result);
    }
}
