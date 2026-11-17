using Microsoft.AspNetCore.Authorization;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.Api.Authorization;

/// <summary>
/// Meets a <see cref="PermissionRequirement"/> when the current user's roles grant the permission. The
/// Administrator role grants every permission.
/// </summary>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserAccessService _userAccess;
    private readonly IHttpContextAccessor _httpContext;

    public PermissionAuthorizationHandler(IUserAccessService userAccess, IHttpContextAccessor httpContext)
    {
        _userAccess = userAccess;
        _httpContext = httpContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var cancellationToken = _httpContext.HttpContext?.RequestAborted ?? CancellationToken.None;
        var access = await _userAccess.GetCurrentAsync(cancellationToken);
        if (access.Has(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
