using System.Net.Mime;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Leaves <c>application/json</c> as the one documented media type where MVC also lists
/// <c>text/json</c>, <c>text/plain</c> and <c>application/*+json</c>, which only repeat the same schema.
/// </summary>
public sealed class JsonOnlyContentTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.RequestBody?.Content is { } request)
        {
            KeepJson(request);
        }

        foreach (var response in operation.Responses ?? [])
        {
            if (response.Value.Content is { } content)
            {
                KeepJson(content);
            }
        }

        return Task.CompletedTask;
    }

    private static void KeepJson(IDictionary<string, OpenApiMediaType> content)
    {
        if (!content.ContainsKey(MediaTypeNames.Application.Json))
        {
            return;
        }

        foreach (var mediaType in content.Keys.Where(key => key != MediaTypeNames.Application.Json).ToList())
        {
            content.Remove(mediaType);
        }
    }
}
