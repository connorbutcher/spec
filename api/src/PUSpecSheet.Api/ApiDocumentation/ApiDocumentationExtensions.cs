using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Scalar.AspNetCore;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// The OpenAPI document and the Scalar page that shows it. The document is at
/// <c>/openapi/v1.json</c> and the interactive documentation at <c>/scalar</c>.
/// </summary>
public static class ApiDocumentationExtensions
{
    public const string DocumentName = "v1";

    public const string ScalarRoute = "/scalar";

    public static IServiceCollection AddPuSpecSheetApiDocumentation(this IServiceCollection services)
    {
        // The document's schemas are built from these options rather than the controllers' own, so they
        // are kept the same: enums as their names, and a "kind" discriminator anywhere in the object.
        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
            options.SerializerOptions.AllowOutOfOrderMetadataProperties = true;
        });

        services.AddOpenApi(DocumentName, options =>
        {
            options.AddDocumentTransformer<ApiInfoDocumentTransformer>();
            options.AddDocumentTransformer<HealthCheckDocumentTransformer>();
            options.AddOperationTransformer<OperationIdTransformer>();
            options.AddOperationTransformer<ConditionalReadOperationTransformer>();
            options.AddOperationTransformer<ProblemResponseDescriptionsTransformer>();
            options.AddOperationTransformer<PublishedSelectionParametersTransformer>();
            options.AddOperationTransformer<JsonOnlyContentTransformer>();
            options.AddSchemaTransformer<MemberReferenceSchemaTransformer>();
            options.AddSchemaTransformer<OmittedWhenNullSchemaTransformer>();
            options.AddSchemaTransformer<PublishedSchemaTransformer>();
        });

        services.Configure<MvcOptions>(options => options.Conventions.Add(new ProblemResponsesConvention()));

        return services;
    }

    public static WebApplication MapPuSpecSheetApiDocumentation(this WebApplication app)
    {
        app.MapOpenApi();
        app.MapScalarApiReference(ScalarRoute, options =>
        {
            options.Title = "PU Spec Sheet API";
            options.Theme = ScalarTheme.Default;
            options.Layout = ScalarLayout.Modern;
            options.DefaultOpenAllTags = false;
            options.HideClientButton = true;
            options.ShowOperationId = true;

            // The page is for reading and trying the API: nothing is sent to Scalar, and its hosted
            // extras (AI chat, MCP generation, the share and deploy toolbar) are switched off.
            options.Telemetry = false;
            options.ShowDeveloperTools = DeveloperToolsVisibility.Never;
            options.Agent = new ScalarAgentOptions { Disabled = true };
            options.Mcp = new ScalarMcpOptions { Disabled = true };
            options.DefaultHttpClient = new KeyValuePair<ScalarTarget, ScalarClient>(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        return app;
    }
}
