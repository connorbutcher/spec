using Microsoft.AspNetCore.OpenApi;
using Microsoft.Net.Http.Headers;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Documents the caching headers of an operation that can answer 304 Not Modified: the
/// <c>If-None-Match</c> request header, and the <c>ETag</c>, <c>Cache-Control</c> and
/// <c>Content-Location</c> response headers. An action opts in by declaring a 304 response.
/// </summary>
public sealed class ConditionalReadOperationTransformer : IOpenApiOperationTransformer
{
    private const string Ok = "200";
    private const string NotModified = "304";

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (operation.Responses is null || !operation.Responses.TryGetValue(NotModified, out var notModified))
        {
            return Task.CompletedTask;
        }

        operation.Parameters ??= [];
        operation.Parameters.Add(new OpenApiParameter
        {
            Name = HeaderNames.IfNoneMatch,
            In = ParameterLocation.Header,
            Required = false,
            Description = "The `ETag` of the answer you already hold. When it still matches, the response is `304 Not Modified` with no body.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
        });

        if (notModified is OpenApiResponse emptyResponse)
        {
            emptyResponse.Description = "The answer you hold is still right. There is no body.";
            emptyResponse.Content?.Clear();
            AddCachingHeaders(emptyResponse);
        }

        if (operation.Responses.TryGetValue(Ok, out var ok) && ok is OpenApiResponse okResponse)
        {
            AddCachingHeaders(okResponse);
        }

        return Task.CompletedTask;
    }

    private static void AddCachingHeaders(OpenApiResponse response)
    {
        response.Headers ??= new Dictionary<string, IOpenApiHeader>();
        response.Headers[HeaderNames.ETag] = Header(
            "Names the version read with this selection. Send it back in `If-None-Match`.");
        response.Headers[HeaderNames.CacheControl] = Header(
            "`private, max-age=31536000, immutable` when the request names a version number or a past moment, "
            + "because that answer never changes. `private, no-cache` otherwise: keep the answer, but check it before using it.");
        response.Headers[HeaderNames.ContentLocation] = Header(
            "Sent when the request didn't name a version number: the address of the version that was returned, "
            + "which always gives this same answer.");
    }

    private static OpenApiHeader Header(string description)
    {
        return new OpenApiHeader
        {
            Description = description,
            Schema = new OpenApiSchema { Type = JsonSchemaType.String },
        };
    }
}
