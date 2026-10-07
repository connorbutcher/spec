using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Contracts.Phases;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Phase endpoints. Reading is open to everyone; changes need the <c>phases.manage</c> permission.</summary>
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

    /// <summary>Create a phase</summary>
    /// <remarks>
    /// Adds a phase after its siblings: under <c>parentPhaseId</c>, or at the top level when that is null.
    /// <c>sheetTypeIds</c> are the sheet types the phase has, which are not inherited from the parent.
    /// Needs the <c>phases.manage</c> permission.
    /// </remarks>
    /// <param name="request">The phase's code, description, parent and sheet types.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The new phase.</response>
    /// <response code="403">The current user doesn't have the <c>phases.manage</c> permission.</response>
    /// <response code="404">The parent phase or one of the sheet types doesn't exist.</response>
    /// <response code="409">Another phase already has this code.</response>
    [HttpPost]
    [Authorize(Policy = PermissionKeys.PhasesManage)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound, ProblemResponseDescriptionsTransformer.ContentType)]
    public async Task<ActionResult<PhaseDto>> Create(CreatePhaseRequest request, CancellationToken cancellationToken)
    {
        var result = await phases.CreateAsync(request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Move a phase</summary>
    /// <remarks>
    /// Moves a phase to a 1-based <c>position</c> among the phases under <c>parentPhaseId</c>, or among the
    /// top-level phases when that is null. Give the phase's current parent to reorder it among its
    /// siblings. The phases beneath it move with it, and the siblings it joins and leaves are renumbered
    /// 1..n. Returns every phase, as the list endpoint does. Needs the <c>phases.manage</c> permission.
    /// </remarks>
    /// <param name="id">The phase's id.</param>
    /// <param name="request">The parent to move under and the position among its phases.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">All phases, with their new parents and display orders.</response>
    /// <response code="400">The phase would go under itself or one of the phases beneath it, or the position is below 1.</response>
    /// <response code="403">The current user doesn't have the <c>phases.manage</c> permission.</response>
    /// <response code="404">The phase or the parent phase doesn't exist.</response>
    [HttpPost("{id:int}/move")]
    [Authorize(Policy = PermissionKeys.PhasesManage)]
    public async Task<ActionResult<IReadOnlyList<PhaseDto>>> Move(int id, MovePhaseRequest request, CancellationToken cancellationToken)
    {
        var result = await phases.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Set a phase's sheet types</summary>
    /// <remarks>
    /// Replaces the sheet types available to a phase with <c>sheetTypeIds</c>. A type left out is removed
    /// from the phase, unless its sheet on this phase already has tables. Needs the <c>phases.manage</c>
    /// permission.
    /// </remarks>
    /// <param name="id">The phase's id.</param>
    /// <param name="request">The full list of sheet types the phase has.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The updated phase.</response>
    /// <response code="403">The current user doesn't have the <c>phases.manage</c> permission.</response>
    /// <response code="404">The phase or one of the sheet types doesn't exist.</response>
    /// <response code="409">A sheet type being removed has a sheet with tables on this phase.</response>
    [HttpPut("{id:int}/sheet-types")]
    [Authorize(Policy = PermissionKeys.PhasesManage)]
    public async Task<ActionResult<PhaseDto>> SetSheetTypes(
        int id,
        SetPhaseSheetTypesRequest request,
        CancellationToken cancellationToken)
    {
        var result = await phases.SetSheetTypesAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
