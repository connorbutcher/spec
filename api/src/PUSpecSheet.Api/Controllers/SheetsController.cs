using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Sheets;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Sheets in the shape of the revision schema, alongside the legacy <c>specsheets</c> endpoints. Ids here are sheet ids
/// and the ids of the sheet's items, not those of a copy.
/// </summary>
[Route("sheets")]
[Authorize]
public class SheetsController : ControllerBase
{
    private readonly ISheetDataAccessService _sheetDataAccessService;

    public SheetsController(ISheetDataAccessService sheetDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(sheetDataAccessService, nameof(sheetDataAccessService));

        _sheetDataAccessService = sheetDataAccessService;
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Sheet>> GetSheetAsync(int id, [FromQuery] int? version = null)
    {
        var sheet = await _sheetDataAccessService.GetPublishedSheetAsync(id, version, HttpContext.RequestAborted);
        return sheet is null ? NotFound() : Ok(sheet);
    }

    [HttpGet("{id:int}/draft")]
    public async Task<ActionResult<Sheet>> GetDraftSheetAsync(int id)
    {
        var sheet = await _sheetDataAccessService.GetDraftSheetAsync(id, HttpContext.RequestAborted);
        return sheet is null ? NotFound() : Ok(sheet);
    }

    [HttpGet("{id:int}/versions")]
    public async Task<ActionResult<IReadOnlyList<SheetVersion>>> GetSheetVersionsAsync(int id)
    {
        var versions = await _sheetDataAccessService.GetSheetVersionsAsync(id, HttpContext.RequestAborted);
        return Ok(versions);
    }
}
