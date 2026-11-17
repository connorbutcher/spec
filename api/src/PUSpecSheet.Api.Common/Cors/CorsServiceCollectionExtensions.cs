using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Api.Common.Cors;

public static class CorsServiceCollectionExtensions
{
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, params string[] allowedDomains)
    {
        services.AddCors(o => o.AddPolicy(CorsConstants.DefaultCorsPolicy, builder =>
        {
            builder.WithOrigins(allowedDomains)
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowCredentials();
        }));

        return services;
    }

    public static IServiceCollection AddCorsPolicy(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        var allowedDomains = configurationSection.GetRequiredSection("AllowedDomains").Get<string[]>();

        if (allowedDomains == null || allowedDomains.Length == 0)
        {
            throw new ArgumentException("AllowedDomains must be specified in the configuration section.");
        }

        return services.AddCorsPolicy(allowedDomains);
    }
}
