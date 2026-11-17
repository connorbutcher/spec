using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PUSpecSheet.Api.Common.Documentation;

public class SwaggerGenConfiguration
{
    public SwaggerGenConfiguration(SwaggerGenOptions swaggerGenOptions, IConfigurationSection swaggerConfigurationSection)
    {
        ArgumentNullException.ThrowIfNull(swaggerGenOptions, nameof(swaggerGenOptions));
        ArgumentNullException.ThrowIfNull(swaggerConfigurationSection, nameof(swaggerConfigurationSection));
        SwaggerGenOptions = swaggerGenOptions;
        SwaggerConfigurationSection = swaggerConfigurationSection;
    }

    public SwaggerGenOptions SwaggerGenOptions { get; }

    public IConfigurationSection SwaggerConfigurationSection { get; }
}
