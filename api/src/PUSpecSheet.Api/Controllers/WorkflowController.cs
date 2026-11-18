using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Requests.Workflow;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

namespace PUSpecSheet.Api.Controllers;

[Route("workflow")]
[Authorize]
public class WorkflowController : ControllerBase
{
    private readonly ISpecSheetDataAccessService _specSheetDataAccessService;

    public WorkflowController(ISpecSheetDataAccessService specSheetDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(specSheetDataAccessService, nameof(specSheetDataAccessService));

        _specSheetDataAccessService = specSheetDataAccessService;
    }

    [Obsolete("Use PATCH sheets/{id}/submission/decision.")]
    [HttpPatch("actor")]
    public async Task<ActionResult> UpdateWorkflowStepActorAsync([FromBody] UpdateWorkflowStepActorRequest updateWorkflowStepActor)
    {
        if (!ModelState.IsValid)
        {

            return BadRequest(ModelState);
        }

        await _specSheetDataAccessService.UpdateWorkflowStepActorStateAsync(updateWorkflowStepActor, HttpContext.RequestAborted);
        return NoContent();
    }

    [Obsolete("Use PATCH sheets/{id}/submission/approvers.")]
    [HttpPatch("reassign")]
    public async Task<ActionResult> ReassignWorkflowStepActorsAsync([FromBody] ReassignWorkflowStepActorsRequest reassignWorkflowStepActorsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _specSheetDataAccessService.ReassignWorkflowStepActorsAsync(reassignWorkflowStepActorsRequest, HttpContext.RequestAborted);
        return NoContent();
    }
}
