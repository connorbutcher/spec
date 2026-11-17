using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.DAL.DataAccess.Admin;

namespace PUSpecSheet.Api.Controllers;

public class AdminController : ControllerBase
{
    private readonly IAdminDataAccessService _adminDataAccessService;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IAdminDataAccessService adminDataAccessService,
        ILogger<AdminController> logger)
    {
        _adminDataAccessService = adminDataAccessService;
        _logger = logger;
    }

    [HttpPost("cache/refresh")]
    public async Task<ActionResult> RefreshCacheAsync([FromQuery] int specSheetTypeId)
    {
        if (specSheetTypeId <= 0)
        {
            _logger.LogError("Attempted to refresh cache with invalid spec sheet type ID: {SpecSheetTypeId}", specSheetTypeId);
            return BadRequest("Invalid spec sheet type ID.");
        }

        await _adminDataAccessService.RefreshCacheForSpecSheetTypeAsync(specSheetTypeId, HttpContext.RequestAborted);
        return Ok();
    }

    [HttpDelete("cache/reset")]
    public async Task<ActionResult> ResetCacheAsync([FromQuery] int specSheetTypeId)
    {
        if (specSheetTypeId <= 0)
        {
            _logger.LogError("Attempted to reset cache with invalid spec sheet type ID: {SpecSheetTypeId}", specSheetTypeId);
            return BadRequest("Invalid spec sheet type ID.");
        }

        await _adminDataAccessService.ResetCacheForSpecSheetTypeAsync(specSheetTypeId, HttpContext.RequestAborted);
        return Ok();
    }
}
