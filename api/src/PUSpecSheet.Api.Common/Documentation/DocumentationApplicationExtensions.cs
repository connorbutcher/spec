using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.Common.Documentation;

public static class DocumentationApplicationExtensions
{
    private const string DefaultSwaggerSectionName = "Swagger";

    public static void UseDocumentation(this WebApplication app, OpenApiInfo apiInfo, string swaggerSectionName = DefaultSwaggerSectionName, Action<SwaggerUIConfiguration>? config = null)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        var configSection = app.Configuration.GetSection(swaggerSectionName);
        if (configSection is not null && configSection.GetValue<bool>("Enabled"))
        {
            app.UseSwagger(configSection, apiInfo, config);
        }
    }

    public static void UseDocumentation(this WebApplication app, OpenApiInfo apiInfo, Action<SwaggerUIConfiguration>? config = null)
    {
        app.UseDocumentation(apiInfo, DefaultSwaggerSectionName, config);
    }

    public static void UseDocumentation(this WebApplication app, OpenApiInfo apiInfo)
    {
        app.UseDocumentation(apiInfo, DefaultSwaggerSectionName, null);
    }
}
