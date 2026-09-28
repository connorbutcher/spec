namespace PUSpecSheet.Api.Cors;

public static class PuSpecSheetCorsExtensions
{
    /// <summary>The CORS policy applied to every endpoint.</summary>
    public const string PolicyName = "PuSpecSheetUi";

    /// <summary>
    /// Registers a named CORS policy that allows only the origins listed in configuration under
    /// <see cref="CorsSettings.SectionName"/>. With no origins configured, no cross-origin calls are allowed.
    /// </summary>
    public static IServiceCollection AddPuSpecSheetCors(this IServiceCollection services, IConfiguration configuration)
    {
        var settings = configuration.GetSection(CorsSettings.SectionName).Get<CorsSettings>() ?? new CorsSettings();
        var origins = settings.AllowedOrigins
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .Select(origin => origin.Trim().TrimEnd('/'))
            .ToArray();

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(origins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        return services;
    }
}
