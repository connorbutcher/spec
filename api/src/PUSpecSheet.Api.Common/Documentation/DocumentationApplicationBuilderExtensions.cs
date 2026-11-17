using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.Common.Documentation;

public static class DocumentationApplicationBuilderExtensions
{
    private const string DefaultSwaggerSectionName = "Swagger";

    public static IHostApplicationBuilder AddDocumentation(
        this IHostApplicationBuilder builder,
        OpenApiInfo apiInfo,
        string swaggerSectionName = DefaultSwaggerSectionName,
        Action<SwaggerGenConfiguration>? config = null)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();

        var swaggerSection = builder.Configuration.GetSection(swaggerSectionName);
        if (swaggerSection is not null && swaggerSection.GetValue<bool>("Enabled"))
        {
            builder.Services.AddSwagger(swaggerSection, apiInfo, config);
        }

        return builder;
    }

    public static IHostApplicationBuilder AddDocumentation(this IHostApplicationBuilder builder, OpenApiInfo apiInfo, Action<SwaggerGenConfiguration>? config = null)
    {
        return builder.AddDocumentation(apiInfo, DefaultSwaggerSectionName, config);
    }

    public static IHostApplicationBuilder AddDocumentation(this IHostApplicationBuilder builder, OpenApiInfo apiInfo)
    {
        return builder.AddDocumentation(apiInfo, DefaultSwaggerSectionName, null);
    }
}
