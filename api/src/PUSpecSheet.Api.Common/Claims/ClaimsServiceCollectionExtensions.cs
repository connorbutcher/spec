using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Api.Common.Context;

namespace PUSpecSheet.Api.Common.Claims;

public static class ClaimsServiceCollectionExtensions
{
    public static IServiceCollection AddClaimsAccessors(this IServiceCollection services)
    {
        services.AddSingleton<IClaimsAccessor, ClaimsAccessor>();
        services.AddSingleton<IIdentityContextAccessor, ClaimsIdentityContextAccessor>();

        return services;
    }

    /// <summary>
    /// Registers accessors that give every request the <see cref="DefaultUser"/>, for running locally with auth
    /// disabled. Use in place of <see cref="AddClaimsAccessors"/>.
    /// </summary>
    public static IServiceCollection AddDefaultUserClaimsAccessors(this IServiceCollection services)
    {
        services.AddSingleton<IClaimsAccessor, DefaultUserClaimsAccessor>();
        services.AddSingleton<IIdentityContextAccessor, ClaimsIdentityContextAccessor>();

        return services;
    }
}
