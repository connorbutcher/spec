using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

namespace PUSpecSheet.Api.Controllers;

[Route("specsheettables")]
[Authorize]
public class SpecSheetTablesController : ControllerBase
{
    private readonly ISpecSheetDataAccessService _specSheetDataAccessService;

    public SpecSheetTablesController(ISpecSheetDataAccessService specSheetDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(specSheetDataAccessService, nameof(specSheetDataAccessService));

        _specSheetDataAccessService = specSheetDataAccessService;
    }

    [HttpGet("all/unique")]
    public async Task<ActionResult<ICollection<SpecSheetTable>>> GetAllUniqueTablesAsync()
    {
        var tables = await _specSheetDataAccessService.GetAllUniqueSpecSheetTablesAsync(HttpContext.RequestAborted);
        return Ok(tables);
    }
}
