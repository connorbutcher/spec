using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// The OpenAPI document and the Swagger UI page that shows it. The document is at
/// <c>/openapi/v1.json</c> and the interactive documentation at <c>/docs</c>.
/// </summary>
public static class ApiDocumentationExtensions
{
    public const string DocumentName = "v1";

    public const string DocumentRoute = "/openapi/v1.json";

    public const string SwaggerUiRoutePrefix = "docs";

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
        app.MapOpenApi().AllowAnonymous();
        app.UseSwaggerUI(options =>
        {
            // Swagger UI only draws the page: the document it shows is the one mapped above.
            options.RoutePrefix = SwaggerUiRoutePrefix;
            options.SwaggerEndpoint(DocumentRoute, "PU Spec Sheet API v1");
            options.DocumentTitle = "PU Spec Sheet API";
            options.DocExpansion(DocExpansion.List);
            options.DefaultModelsExpandDepth(0);
            options.DisplayOperationId();
            options.DisplayRequestDuration();
            options.EnableDeepLinking();
            options.EnableFilter();
            options.EnableTryItOutByDefault();
        });

        return app;
    }
}
