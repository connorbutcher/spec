using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.DAL.DataAccess.Query;
using PUSpecSheet.Query.Models;
using PUSpecSheet.Query.Models.Phase;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.Controllers;

[Route("query")]
[Authorize]
public class QueryController : ControllerBase
{
    private readonly IQueryDataAccessService _queryDataAccessService;
    private readonly ILogger<QueryController> _logger;

    public QueryController(IQueryDataAccessService queryDataAccessService,
        ILogger<QueryController> logger)
    {
        ArgumentNullException.ThrowIfNull(queryDataAccessService, nameof(queryDataAccessService));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _queryDataAccessService = queryDataAccessService;
        _logger = logger;
    }

    /// <summary>
    /// Gets all the spec sheet types.
    /// </summary>    
    /// <returns>An ActionResult containing all the spec sheet types, or an error response if no types are found or an internal error occurs.</returns>
    [HttpGet("v1/types/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ICollection<SlimPUSpecSheetType>>> GetAllSpecSheetTypesAsync()
    {
        try
        {
            var specSheetTypes = await _queryDataAccessService.GetAllSpecSheetTypesAsync(HttpContext.RequestAborted);
            if (specSheetTypes == null || specSheetTypes.Count == 0)
            {
                return NoContent();
            }

            return Ok(specSheetTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for all spec sheet phases. Message: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all the spec sheet phases. 
    /// </summary>
    /// <remarks>
    /// This endpoint returns the spec sheet phases in a hierarchical structure, where each phase includes its child phases and the types supported for that phase.    
    /// </remarks>
    /// <returns>An ActionResult containing the spec sheet phases in a hierarchical structure, or an error response if no phases are found or an internal error occurs.</returns>
    [HttpGet("v1/phases/all")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ICollection<SlimSpecSheetPhaseWithChildren>>> GetAllSpecSheetPhasesAsync()
    {
        try
        {
            var specSheetPhases = await _queryDataAccessService.GetAllSpecSheetPhasesAsync(HttpContext.RequestAborted);
            if (specSheetPhases == null || specSheetPhases.Count == 0)
            {
                return NoContent();
            }

            return Ok(specSheetPhases);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for all spec sheet phases. Message: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the spec sheet by id.
    /// </summary>
    /// <remarks>This will ONLY return published spec sheets.</remarks>
    /// <param name="specSheetId">The id of the spec sheet.</param>
    /// <returns>An ActionResult containing the spec sheet with the specified id, or an error response if the request is invalid, the sheet is not found, or an internal error occurs.</returns>
    [HttpGet("v1/specsheet/{specSheetId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SlimPUSpecSheet>> GetSpecSheetByIdAsync([FromRoute] int specSheetId)
    {
        if (specSheetId <= 0)
        {
            return BadRequest("Spec sheet id must be greater than 0.");
        }

        try
        {
            var specSheet = await _queryDataAccessService.GetSpecSheetByIdAsync(specSheetId, HttpContext.RequestAborted);
            if (specSheet == null)
            {
                return NotFound($"Spec sheet with id {specSheetId} not found.");
            }

            return Ok(specSheet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for spec sheet id {SpecSheetId}. Message: {Message}", specSheetId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all the spec sheets for the specified type id. This includes all the phases for that type.
    /// </summary>
    /// <remarks>This will ONLY return published spec sheets.</remarks>
    /// <param name="typeId">The id of the spec sheet type.</param>
    /// <returns>An ActionResult containing the spec sheets for the specified type id, or an error response if the request is invalid, no matching sheets are found, or an internal error occurs.</returns>
    [HttpGet("v1/specsheet/all/type/{typeId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ICollection<SlimPUSpecSheet>>> GetAllSpecSheetsByTypeAsync([FromRoute] int typeId)
    {
        if (typeId <= 0)
        {
            return BadRequest("Spec sheet type id must be greater than 0.");
        }

        try
        {
            var specSheets = await _queryDataAccessService.GetAllSpecSheetsByTypeAsync(typeId, HttpContext.RequestAborted);
            if (specSheets == null || specSheets.Count == 0)
            {
                return NotFound($"Spec sheets not found for type id {typeId}.");
            }

            return Ok(specSheets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for spec sheets by type id {TypeId}. Message: {Message}", typeId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all the spec sheets for the specified type id and phase id.
    /// </summary>
    /// <remarks>This will ONLY return published spec sheets.</remarks>
    /// <param name="typeId">The id of the spec sheet type.</param>
    /// <param name="phaseId">The id of the spec sheet phase.</param>
    /// <returns>An ActionResult containing the spec sheets for the specified type id and phase id, or an error response if the request is invalid, no matching sheets are found, or an internal error occurs.</returns>
    [HttpGet("v1/specsheet/all/type/{typeId:int}/phase/{phaseId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ICollection<SlimPUSpecSheet>>> GetAllSpecSheetsByTypeAndPhaseAsync([FromRoute] int typeId, [FromRoute] int phaseId)
    {
        if (typeId <= 0)
        {
            return BadRequest("Spec sheet type id must be greater than 0.");
        }

        if (phaseId <= 0)
        {
            return BadRequest("Spec sheet phase id must be greater than 0.");
        }

        try
        {
            var specSheets = await _queryDataAccessService.GetAllSpecSheetsByTypeAndPhaseAsync(typeId, phaseId, HttpContext.RequestAborted);
            if (specSheets == null || specSheets.Count == 0)
            {
                return NotFound($"Spec sheets not found for type id {typeId} and phase id {phaseId}.");
            }

            return Ok(specSheets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for spec sheets by type id {TypeId} and phase id {PhaseId}. Message: {Message}", typeId, phaseId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Retrieves the most recently published specification sheet for the specified type and phase.
    /// </summary>
    /// <remarks>This will ONLY return published spec sheets.</remarks>
    /// <param name="typeId">The unique identifier of the specification sheet type. Must be greater than 0.</param>
    /// <param name="phaseId">The unique identifier of the specification sheet phase. Must be greater than 0.</param>
    /// <param name="publishedByDateTime">An optional date and time used to filter the published specification sheets. If specified, only sheets published
    /// on or before this date are considered.</param>
    /// <returns>An ActionResult containing the latest published SlimPUSpecSheet for the given type and phase, or an error
    /// response if the request is invalid, no matching sheet is found, or an internal error occurs.</returns>
    [HttpGet("v1/specsheet/latest/published/type/{typeId:int}/phase/{phaseId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SlimPUSpecSheet>> GetLatestPublishedSpecSheetByPhaseAndTypeAsync([FromRoute] int typeId, [FromRoute] int phaseId, [FromQuery] DateTime? publishedByDateTime)
    {
        if (typeId <= 0)
        {
            return BadRequest("Spec sheet type id must be greater than 0.");
        }
        if (phaseId <= 0)
        {
            return BadRequest("Spec sheet phase id must be greater than 0.");
        }

        try
        {
            var specSheet = await _queryDataAccessService.GetLatestPublishedSpecSheetByPhaseAndTypeAsync(typeId, phaseId, publishedByDateTime, HttpContext.RequestAborted);
            if (specSheet == null)
            {
                return NotFound($"No published spec sheet found for type id {typeId} and phase id {phaseId}.");
            }

            return Ok(specSheet);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for latest published spec sheet by type id {TypeId} and phase id {PhaseId}. Message: {Message}", typeId, phaseId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets all the spec sheets for the specified type id and phase id that were published after the specified date and time.
    /// </summary>
    /// <remarks>This will ONLY return published spec sheets.</remarks>
    /// <param name="typeId">The unique identifier of the specification sheet type. Must be greater than 0.</param>
    /// <param name="phaseId">The unique identifier of the specification sheet phase. Must be greater than 0.</param>
    /// <param name="publishedAfterDateTime">The date and time after which the specification sheets were published. Must be a valid DateTime.</param>
    /// <returns>An ActionResult containing a collection of SlimPUSpecSheetSummary objects that match the specified criteria, or an error response if the request is invalid, no matching sheets are found, or an internal error occurs.</returns>
    [HttpGet("v1/specsheet/published/type/{typeId:int}/phase/{phaseId:int}/afterdate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<ICollection<SlimPUSpecSheetSummary>>> GetPublishedSpecSheetsByPhaseAndTypeAfterDateAsync([FromRoute] int typeId, [FromRoute] int phaseId, [FromQuery] DateTime publishedAfterDateTime)
    {
        if (typeId <= 0)
        {
            return BadRequest("Spec sheet type id must be greater than 0.");
        }
        if (phaseId <= 0)
        {
            return BadRequest("Spec sheet phase id must be greater than 0.");
        }

        try
        {
            var specSheets = await _queryDataAccessService.GetPublishedSpecSheetsByPhaseAndTypeAfterDateAsync(typeId, phaseId, publishedAfterDateTime, HttpContext.RequestAborted);
            if (specSheets == null || specSheets.Count == 0)
            {
                return NotFound($"No published spec sheets found for type id {typeId} and phase id {phaseId} after {publishedAfterDateTime}.");
            }

            return Ok(specSheets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while processing the request for published spec sheets by type id {TypeId} and phase id {PhaseId} after date {PublishedAfterDateTime}. Message: {Message}", typeId, phaseId, publishedAfterDateTime, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Updates the collection of external system references with the provided data. This is a PUT operation so any references not included in the input collection will be removed, and the provided references will be added or updated as necessary to match the input collection. The method ensures that the external system references are synchronized with the provided data, allowing for both additions and removals based on the input.
    /// </summary>
    /// <remarks>If the input collection is null or empty, the method returns a 400 Bad Request response. If
    /// an internal error occurs during the update process, a 500 Internal Server Error response is returned with an
    /// error message.</remarks>
    /// <param name="systemReferences">A collection of external system reference requests to update. This parameter must not be null or empty.</param>
    /// <returns>An ActionResult indicating the outcome of the update operation. Returns 200 OK if the update is successful;
    /// otherwise, returns an appropriate error response.</returns>
    [HttpPut("v1/systemreferences")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> UpdateSystemReferencesAsync([FromBody] ICollection<ExternalSystemReferenceRequest> systemReferences)
    {
        if (systemReferences == null || systemReferences.Count == 0)
        {
            return BadRequest("System references must be provided.");
        }

        try
        {
            await _queryDataAccessService.UpdateSystemReferencesAsync(systemReferences, HttpContext.RequestAborted);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while updating system references. Message: {Message}", ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }

    /// <summary>
    /// Deletes all system references associated with the specified external identifier.
    /// </summary>
    /// <remarks>Returns a 400 Bad Request response if the provided externalId is less than or equal to 0.
    /// Returns a 500 Internal Server Error if an unexpected error occurs during the deletion process.</remarks>
    /// <param name="externalId">The external identifier for which to delete system references. Must be greater than 0.</param>
    /// <returns>An ActionResult that indicates the outcome of the operation. Returns 200 OK if the deletion is successful;
    /// otherwise, returns an error response.</returns>
    [HttpDelete("v1/systemreferences/external/{externalId:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> DeleteSystemReferencesByExternalIdAsync([FromRoute] int externalId)
    {
        if (externalId <= 0)
        {
            return BadRequest("External id must be greater than 0.");
        }
        try
        {
            await _queryDataAccessService.DeleteSystemReferencesByExternalIdAsync(externalId, HttpContext.RequestAborted);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while deleting system references for external id {ExternalId}. Message: {Message}", externalId, ex.Message);
            return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while processing your request: {ex.Message}");
        }
    }
}