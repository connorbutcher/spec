using Microsoft.AspNetCore.Authorization;
using PUSpecSheet.Api.Users;
using PUSpecSheet.Application.Users;
using PUSpecSheet.Domain.Users;

namespace PUSpecSheet.Api.Authorization;

public static class PuSpecSheetAuthorizationExtensions
{
    /// <summary>
    /// Registers who a request runs as and what they may do. There is one authorization policy per
    /// permission, named by the permission's key, so an action asks for one with
    /// <c>[Authorize(Policy = PermissionKeys.PhasesManage)]</c>.
    /// </summary>
    public static IServiceCollection AddPuSpecSheetAuthorization(this IServiceCollection services)
    {
        // Until sign-in is added, every request runs as the seeded developer user. Real sign-in replaces
        // these two registrations: an authentication scheme, and an ICurrentUser that reads its claims.
        services.AddScoped<ICurrentUser, DeveloperCurrentUser>();
        services.AddAuthentication(DeveloperAuthenticationHandler.SchemeName)
            .AddScheme<DeveloperAuthenticationOptions, DeveloperAuthenticationHandler>(DeveloperAuthenticationHandler.SchemeName, null);

        services.AddHttpContextAccessor();
        services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddSingleton<IAuthorizationMiddlewareResultHandler, PermissionDeniedResultHandler>();

        var authorization = services.AddAuthorizationBuilder();
        foreach (var permission in PermissionKeys.All)
        {
            authorization.AddPolicy(permission, policy => policy.AddRequirements(new PermissionRequirement(permission)));
        }

        return services;
    }
}
