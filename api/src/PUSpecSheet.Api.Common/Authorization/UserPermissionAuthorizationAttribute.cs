using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Api.Common.Context;

namespace PUSpecSheet.Api.Common.Authorization;

public sealed class UserPermissionAuthorizationAttribute : AuthorizeAttribute, IAsyncAuthorizationFilter
{
    private readonly string[] _access;

    public UserPermissionAuthorizationAttribute(params string[] access)
    {
        _access = access;
    }

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var isAuthenticated = context.HttpContext.User.Identity?.IsAuthenticated ?? false;
        if (!isAuthenticated)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var userContext = context.HttpContext.RequestServices.GetService<IUserContext>();
        if (userContext == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var user = await userContext.EnsureUserAsync(context.HttpContext.RequestAborted);
        if (user == null)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var hasPermission = user.Permissions.Any(p => _access.Contains(p.Permission!.Key));
        if (!hasPermission)
        {
            context.Result = new ForbidResult();
            return;
        }
    }
}
