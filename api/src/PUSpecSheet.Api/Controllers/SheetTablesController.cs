using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Contracts.Common;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Controllers;

/// <summary>Changes to one table on a sheet. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Tags(ApiTags.SheetTables)]
[Route("api/sheet-tables")]
public sealed class SheetTablesController(ISheetTableService tables, ISheetSectionService sections) : ControllerBase
{
    /// <summary>Set a table's title</summary>
    /// <remarks>
    /// Sets the table's title. An empty title falls back to the template's name.
    /// </remarks>
    /// <param name="id">The sheet table's id.</param>
    /// <param name="request">The new title.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<SheetDto>> SetTitle(int id, UpdateSheetTableRequest request, CancellationToken cancellationToken)
    {
        var result = await tables.SetTitleAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Move a table</summary>
    /// <remarks>
    /// Moves the table to a position among the sheet's tables.
    /// </remarks>
    /// <param name="id">The sheet table's id.</param>
    /// <param name="request">The 1-based position to move to. A position past the end moves it last.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpPost("{id:int}/move")]
    public async Task<ActionResult<SheetDto>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await tables.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Remove a table</summary>
    /// <remarks>
    /// Removes the table and everything in it. A table nobody has published is simply deleted; otherwise the removal is your draft until you publish.
    /// </remarks>
    /// <param name="id">The sheet table's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpDelete("{id:int}")]
    public async Task<ActionResult<SheetDto>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await tables.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }

    /// <summary>Add a section to a table</summary>
    /// <remarks>
    /// Adds a copy of an addable template section, with its rows and starting sub-sections, within the
    /// template's limits. It goes under the table, or under the section copy named by
    /// <c>parentSheetSectionId</c>, which must be a copy of the template section's own parent.
    /// </remarks>
    /// <param name="id">The sheet table's id.</param>
    /// <param name="request">The template section to copy and where to put it.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="409">The table is locked by someone else, or already holds the most copies the template allows.</response>
    [HttpPost("{id:int}/sections")]
    public async Task<ActionResult<SheetDto>> AddSection(int id, AddSheetSectionRequest request, CancellationToken cancellationToken)
    {
        var result = await sections.AddAsync(id, request, cancellationToken);
        return Ok(result);
    }
}
