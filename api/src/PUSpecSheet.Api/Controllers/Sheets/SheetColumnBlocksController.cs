using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Common;
using PUSpecSheet.Abstractions.Sheets;
using PUSpecSheet.Abstractions.Users;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.DataAccess.Sheets.Editing;

namespace PUSpecSheet.Api.Controllers.Sheets;

/// <summary>Changes to the column block copies on a horizontal sheet table. Every action returns the refreshed live view of the sheet.</summary>
[ApiController]
[Tags(ApiTags.SheetColumnBlocks)]
[Route("api")]
[Authorize(Policy = PermissionKeys.SheetsEdit)]
public sealed class SheetColumnBlocksController : ControllerBase
{
    private readonly ISheetColumnBlocksService _columnBlocks;

    public SheetColumnBlocksController(ISheetColumnBlocksService columnBlocks)
    {
        _columnBlocks = columnBlocks;
    }

    /// <summary>Add a column block to a table</summary>
    /// <remarks>
    /// Adds a copy of a template column block to the right of the existing ones, within the template's limits. Every row of the table gets its cells in it.
    /// </remarks>
    /// <param name="tableId">The sheet table's id. The table must be horizontal.</param>
    /// <param name="request">The template column block to copy.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="409">The table is locked by someone else, or already holds the most copies the template allows.</response>
    [HttpPost("sheet-tables/{tableId:int}/column-blocks")]
    public async Task<ActionResult<Sheet>> Add(int tableId, AddSheetColumnBlockRequest request, CancellationToken cancellationToken)
    {
        var result = await _columnBlocks.AddAsync(tableId, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Move a column block</summary>
    /// <remarks>
    /// Moves the column block copy to a position among the table's column blocks, counted from the left.
    /// </remarks>
    /// <param name="id">The sheet column block's id.</param>
    /// <param name="request">The 1-based position to move to. A position past the end moves it last.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    [HttpPost("sheet-column-blocks/{id:int}/move")]
    public async Task<ActionResult<Sheet>> Move(int id, MoveRequest request, CancellationToken cancellationToken)
    {
        var result = await _columnBlocks.MoveAsync(id, request, cancellationToken);
        return Ok(result);
    }

    /// <summary>Remove a column block</summary>
    /// <remarks>
    /// Removes the column block copy with its cells in every row. It can't go below the fewest copies its template allows.
    /// </remarks>
    /// <param name="id">The sheet column block's id.</param>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The refreshed live view of the sheet.</response>
    /// <response code="409">The table is locked by someone else, or the block is at the template's minimum.</response>
    [HttpDelete("sheet-column-blocks/{id:int}")]
    public async Task<ActionResult<Sheet>> Remove(int id, CancellationToken cancellationToken)
    {
        var result = await _columnBlocks.RemoveAsync(id, cancellationToken);
        return Ok(result);
    }
}
