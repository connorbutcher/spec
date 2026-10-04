using System.Net.Mime;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Adds the health check to the document. It is mapped as middleware rather than a controller, so the
/// API explorer doesn't know about it.
/// </summary>
public sealed class HealthCheckDocumentTransformer : IOpenApiDocumentTransformer
{
    public const string Path = "/api/health";

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        var operation = new OpenApiOperation
        {
            OperationId = "Health_Check",
            Summary = "Check the API and its database",
            Description = "Answers `Healthy` when the API is running and can reach its database. The UI's header shows the result.",
            Tags = new HashSet<OpenApiTagReference> { new(ApiTags.Health, document) },
            Responses = new OpenApiResponses
            {
                ["200"] = StatusResponse("The API and its database are reachable.", "Healthy"),
                ["503"] = StatusResponse("The database can't be reached.", "Unhealthy"),
            },
        };

        document.Paths[Path] = new OpenApiPathItem
        {
            Operations = new Dictionary<HttpMethod, OpenApiOperation> { [HttpMethod.Get] = operation },
        };

        return Task.CompletedTask;
    }

    private static OpenApiResponse StatusResponse(string description, string status)
    {
        return new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                [MediaTypeNames.Text.Plain] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema { Type = JsonSchemaType.String },
                    Example = JsonValue.Create(status),
                },
            },
        };
    }
}
