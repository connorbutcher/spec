using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.DAL.DataAccess.Query;
using PUSpecSheet.Query.Models.Analytics;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.Controllers.Connections;

[Route("analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IQueryDataAccessService _queryDataAccessService;
    private readonly ILogger<AnalyticsController> _logger;

    public AnalyticsController(IQueryDataAccessService queryDataAccessService,
        ILogger<AnalyticsController> logger)
    {
        _queryDataAccessService = queryDataAccessService;
        _logger = logger;
    }

    [HttpPost("v1/dataset/data")]
    public async Task<ActionResult<ICollection<DataPointResult>>> GetDatasetDataAsync([FromBody] DatasetRequest datasetRequest)
    {
        try
        {
            var cellValues = await _queryDataAccessService.GetDatasetDataAsync(datasetRequest, HttpContext.RequestAborted);
            if (cellValues is null || cellValues.Count == 0)
            {
                return Ok(Array.Empty<DataPointResult>());
            }
            return Ok(cellValues);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while retrieving cell values.");
            return StatusCode(500, "An error occurred while retrieving cell values.");
        }
    }
}
