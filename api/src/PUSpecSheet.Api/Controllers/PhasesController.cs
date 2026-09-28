using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Contracts.Phases;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Read-only phase endpoints. Adding, editing and deleting phases will come with the admin section.</summary>
[ApiController]
[Route("api/phases")]
public sealed class PhasesController(IPhaseService phases) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PhaseDto>>> GetAll(CancellationToken cancellationToken)
    {
        var result = await phases.GetAllAsync(cancellationToken);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PhaseDto>> Get(int id, CancellationToken cancellationToken)
    {
        var result = await phases.GetAsync(id, cancellationToken);
        return Ok(result);
    }
}
