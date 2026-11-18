// using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Requests.Access;
// using PUSpecSheet.Api.Common.Authorization;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.DAL.DataAccess.Admin;

namespace PUSpecSheet.Api.Controllers;

/// <summary>
/// Defines the user controller.
/// </summary>
[Route("users")]
// [Authorize]
public class UserController : ControllerBase
{
    private readonly IUserContext _userContext;
    private readonly IAdminDataAccessService _adminDataAccessService;

    public UserController(IUserContext userContext,
        IAdminDataAccessService adminDataAccessService)
    {
        ArgumentNullException.ThrowIfNull(userContext, nameof(userContext));
        ArgumentNullException.ThrowIfNull(adminDataAccessService, nameof(adminDataAccessService));

        _userContext = userContext;
        _adminDataAccessService = adminDataAccessService;
    }

    /// <summary>
    /// Get the current user.
    /// </summary>        
    /// <returns>The current user.</returns>
    [HttpGet("current")]
    public async Task<ActionResult<User>> GetCurrentUserAsync()
    {
        var user = await _userContext.EnsureUserAsync(HttpContext.RequestAborted);
        return Ok(user);
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    /// <returns>All the users.</returns>
    [HttpGet("all")]
    // [UserPermissionAuthorization("admin.view", "admin.users")]
    public async Task<ActionResult<IReadOnlyList<User>>> GetAllUsersAsync()
    {
        var users = await _adminDataAccessService.GetAllUsersAsync(HttpContext.RequestAborted);
        return Ok(users);
    }

    [HttpPost("create")]
    public async Task<ActionResult> CreateUserAsync([FromBody] CreateUserRequest createUserRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var userId = await _adminDataAccessService.CreateUserAsync(createUserRequest, HttpContext.RequestAborted);
        return Ok(userId);
    }

    /// <summary>
    /// Update the users permissions
    /// </summary>
    /// <returns>All the users.</returns>
    [HttpPatch("permissions/update")]
    // [UserPermissionAuthorization("admin.view", "admin.users")]
    public async Task<ActionResult<IReadOnlyList<User>>> UpdateUserPermissionsAsync([FromBody] UpdateUserPermissionsRequest updateUserPermissionsRequest)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        await _adminDataAccessService.UpdateUserPermissionsAsync(updateUserPermissionsRequest, HttpContext.RequestAborted);
        return Ok();
    }
}
