// using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Template;
using PUSpecSheet.Api.DAL.DataAccess.Template;

namespace PUSpecSheet.Api.Controllers;

[Route("template")]
// [Authorize]
public class TemplateController : ControllerBase
{
    private readonly ITemplateDataAccessService _templateDataAccessService;

    public TemplateController(ITemplateDataAccessService templateDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(templateDataAccessService);

        _templateDataAccessService = templateDataAccessService;
    }

    [HttpGet("celltypes/all")]
    public async Task<ActionResult<IReadOnlyList<TableCellType>>> GetAllTableCellTypesAsync()
    {
        var tableCellTypes = await _templateDataAccessService.GetAllTableCellTypesAsync(HttpContext.RequestAborted);
        return Ok(tableCellTypes);
    }
}
