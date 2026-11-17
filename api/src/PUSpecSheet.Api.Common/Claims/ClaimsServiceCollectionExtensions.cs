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
}
