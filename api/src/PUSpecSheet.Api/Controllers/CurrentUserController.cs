using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Contracts.Users;

namespace PUSpecSheet.Api.Controllers;

[ApiController]
[Tags(ApiTags.CurrentUser)]
[Route("api/me")]
public sealed class CurrentUserController(IUserAccessService userAccess) : ControllerBase
{
    /// <summary>Get the current user</summary>
    /// <remarks>
    /// Who the request runs as, their roles and everything those roles allow. <c>permissions</c> lists the
    /// keys the user holds, such as <c>phases.manage</c>; an administrator holds every one. Use it to hide
    /// or disable what the user can't do: the API refuses those requests with <c>403</c> either way.
    /// </remarks>
    /// <param name="cancellationToken">Cancels the request.</param>
    /// <response code="200">The current user and their permissions.</response>
    [HttpGet]
    public async Task<ActionResult<CurrentUserDto>> Get(CancellationToken cancellationToken)
    {
        var result = await userAccess.GetCurrentUserAsync(cancellationToken);
        return Ok(result);
    }
}
