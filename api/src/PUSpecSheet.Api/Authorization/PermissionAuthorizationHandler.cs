using Microsoft.AspNetCore.Authorization;
using PUSpecSheet.Application.Users;

namespace PUSpecSheet.Api.Authorization;

/// <summary>
/// Meets a <see cref="PermissionRequirement"/> when the current user's roles grant the permission. The
/// Administrator role grants every permission.
/// </summary>
public sealed class PermissionAuthorizationHandler
    : AuthorizationHandler<PermissionRequirement>
{
    private readonly IUserAccessService userAccess;
    private readonly IHttpContextAccessor httpContext;

    public PermissionAuthorizationHandler(IUserAccessService userAccess, IHttpContextAccessor httpContext)
    {
        this.userAccess = userAccess;
        this.httpContext = httpContext;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
    {
        var cancellationToken = httpContext.HttpContext?.RequestAborted ?? CancellationToken.None;
        var access = await userAccess.GetCurrentAsync(cancellationToken);
        if (access.Has(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
