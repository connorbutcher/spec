using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Requests.Access.Tables;
// using PUSpecSheet.Api.Common.Authorization;
using PUSpecSheet.Api.DAL.DataAccess.Admin;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Defines the permissions controller.
/// </summary>
[Route("permissions")]
// [UserPermissionAuthorization("admin.view", "admin.users")]
public class PermissionsController : ControllerBase
{
    private readonly IAdminDataAccessService _adminDataAccessService;

    public PermissionsController(IAdminDataAccessService adminDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(adminDataAccessService, nameof(adminDataAccessService));

        _adminDataAccessService = adminDataAccessService;
    }

    /// <summary>
    /// Get all the permissions.
    /// </summary>
    /// <returns>The permissions.</returns>
    [HttpGet("all")]
    public async Task<ActionResult<IReadOnlyList<Permission>>> GetPermissionsAsync()
    {
        var permissions = await _adminDataAccessService.GetPermissions();
        return Ok(permissions);
    }

    /// <summary>
    /// Get the permissions for a specific table.
    /// </summary>
    /// <param name="referenceId">The reference id of the table.</param>
    /// <returns>The table permission.</returns>
    [HttpGet("tablepermissions/{referenceId:guid}")]
    public async Task<ActionResult<IReadOnlyList<SpecSheetTablePermission>>> GetTablePermissionsByReferenceIdAsync([FromRoute] Guid referenceId)
    {
        var permissions = await _adminDataAccessService.GetTablePermissionsByReferenceIdAsync(referenceId, HttpContext.RequestAborted);
        return Ok(permissions);
    }

    [HttpPatch("tablepermissions/update")]
    public async Task<ActionResult> UpdateTablePermissionsAsync([FromBody] ManageTablePermissionsRequest manageTablePermissionsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _adminDataAccessService.UpdateTablePermissionsAsync(manageTablePermissionsRequest, HttpContext.RequestAborted);
        return Ok();
    }

    /// <summary>
    /// Delete the table permissions for a specific table.
    /// </summary>
    /// <param name="referenceId">The reference id of the table.</param>    
    [HttpDelete("tablepermissions/{referenceId:guid}")]
    public async Task<ActionResult> DeleteTablePermissionsByReferenceIdAsync([FromRoute] Guid referenceId)
    {
        await _adminDataAccessService.DeleteTablePermissionsByReferenceIdAsync(referenceId, HttpContext.RequestAborted);
        return Ok();
    }
}
