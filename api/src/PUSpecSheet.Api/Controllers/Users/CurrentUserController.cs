using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Abstractions.Users;
using PUSpecSheet.Api.ApiDocumentation;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.Api.Controllers.Users;

[ApiController]
[Tags(ApiTags.CurrentUser)]
[Route("api/me")]
public sealed class CurrentUserController : ControllerBase
{
    private readonly IUserAccessService _userAccess;

    public CurrentUserController(IUserAccessService userAccess)
    {
        _userAccess = userAccess;
    }

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
        var result = await _userAccess.GetCurrentUserAsync(cancellationToken);
        return Ok(result);
    }
}
