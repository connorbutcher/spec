using Microsoft.Extensions.Configuration;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace PUSpecSheet.Api.Common.Documentation;

public class SwaggerUIConfiguration
{
    public SwaggerUIConfiguration(SwaggerUIOptions options, IConfigurationSection swaggerSection)
    {
        ArgumentNullException.ThrowIfNull(options, nameof(options));
        ArgumentNullException.ThrowIfNull(swaggerSection, nameof(swaggerSection));
        Options = options;
        SwaggerSection = swaggerSection;
    }

    public SwaggerUIOptions Options { get; }
    public IConfigurationSection SwaggerSection { get; }
}
