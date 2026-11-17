using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;
using PUSpecSheet.Api.Common.Documentation;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PUSpecSheet.Api.Common.Documentation;

public static class Swagger
{
    public static void AddSwagger(this IServiceCollection services, IConfigurationSection swaggerSection, OpenApiInfo apiInfo, Action<SwaggerGenConfiguration>? config)
    {
        services.AddSwaggerGen(swaggerGen =>
        {
            swaggerGen.SwaggerDoc("v1", apiInfo);

            swaggerGen.IncludeAllXmlComments();

            swaggerGen.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
            swaggerGen.CustomSchemaIds(x => x.FullName);

            if (config != null)
            {
                var swaggerConfiguration = new SwaggerGenConfiguration(swaggerGen, swaggerSection);
                config(swaggerConfiguration);
            }
        });
    }

    public static SwaggerGenConfiguration WithEntraIDAuthentication(this SwaggerGenConfiguration swaggerConfiguration, string jwtBearerScheme)
    {
        ArgumentNullException.ThrowIfNull(swaggerConfiguration, nameof(swaggerConfiguration));
        ArgumentException.ThrowIfNullOrWhiteSpace(jwtBearerScheme, nameof(jwtBearerScheme));

        var swaggerGen = swaggerConfiguration.SwaggerGenOptions;
        var swaggerSection = swaggerConfiguration.SwaggerConfigurationSection;

        swaggerGen.OperationFilter<SecurityRequirementsOperationFilter>();

        var authorizationUrl = swaggerSection.GetValue<string>("AuthorizationUrl")!;
        var tokenUrl = swaggerSection.GetValue<string>("TokenUrl")!;

        var scopes = GetScopesDictionary(swaggerSection.GetRequiredSection("Scopes"));

        swaggerGen.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.OAuth2,
            Description = "Microsoft Entra ID Authentication",
            Scheme = jwtBearerScheme,
            Flows = new OpenApiOAuthFlows
            {
                Implicit = new OpenApiOAuthFlow
                {
                    AuthorizationUrl = new Uri(authorizationUrl),
                    TokenUrl = new Uri(tokenUrl),
                    Scopes = scopes
                },
            }
        });

        return swaggerConfiguration;
    }

    public static void UseSwagger(this IApplicationBuilder applicationBuilder, IConfigurationSection swaggerSection, OpenApiInfo apiInfo, Action<SwaggerUIConfiguration>? config)
    {
        applicationBuilder.UseSwaggerUI(options =>
        {
            options.RoutePrefix = "docs";
            options.DocumentTitle = $"{apiInfo.Title} Docs";
            options.HeadContent = "<style>.topbar {display: none;}</style>";

            options.SwaggerEndpoint("v1/docs.json", $"{apiInfo.Title}");

            if (config != null)
            {
                var swaggerConfiguration = new SwaggerUIConfiguration(options, swaggerSection);
                config(swaggerConfiguration);
            }
        });

        applicationBuilder.UseSwagger(options =>
        {
            var endpointOptions = new SwaggerEndpointOptions();

            options.RouteTemplate = "docs/{documentName}/docs.json";
            options.PreSerializeFilters.AddRange(endpointOptions.PreSerializeFilters);
        });
    }

    public static SwaggerUIConfiguration WithEntraIDAuthentication(this SwaggerUIConfiguration swaggerUIConfiguration)
    {
        ArgumentNullException.ThrowIfNull(swaggerUIConfiguration);

        var swaggerSection = swaggerUIConfiguration.SwaggerSection;
        var options = swaggerUIConfiguration.Options;

        //Set the default client id
        var clientId = swaggerSection.GetValue<string>("OAuthClientId")!;
        options.OAuthClientId(clientId);

        //Set the default selected scopes
        var defaultScopes = GetScopesDictionary(swaggerSection.GetRequiredSection("Scopes"));
        options.OAuthScopes([.. defaultScopes.Keys]);

        return swaggerUIConfiguration;
    }

    /// <summary>
    /// Disables syntax highlighting in the Swagger UI.
    /// This can significantly improve performance for larger JSON responses.
    /// </summary>
    public static SwaggerUIConfiguration WithNoSyntaxHighlight(this SwaggerUIConfiguration swaggerUIConfiguration)
    {
        ArgumentNullException.ThrowIfNull(swaggerUIConfiguration);

        var options = swaggerUIConfiguration.Options;
        options.ConfigObject.AdditionalItems.Add("syntaxHighlight", false);

        return swaggerUIConfiguration;
    }

    private static SwaggerGenOptions IncludeAllXmlComments(this SwaggerGenOptions swagger)
    {
        var entryAssembly = Assembly.GetEntryAssembly();
        var workingDirectory = Path.GetDirectoryName(entryAssembly?.Location);

        foreach (var xmlFile in Directory.GetFiles(workingDirectory ?? "", "*.xml", SearchOption.AllDirectories))
        {
            // If there's an exe or dll with a matching filename, this is an XmlDoc file
            if (File.Exists(Path.ChangeExtension(xmlFile, ".dll")) || File.Exists(Path.ChangeExtension(xmlFile, ".exe")))
            {
                swagger.IncludeXmlComments(xmlFile);
            }
        }

        return swagger;
    }

    private static Dictionary<string, string> GetScopesDictionary(IConfigurationSection scopesSection)
    {
        return scopesSection.GetChildren()
            .Select(x => x.Get<string[]>()!)
            .ToDictionary(x => x[0], x => x[1]);
    }
}
