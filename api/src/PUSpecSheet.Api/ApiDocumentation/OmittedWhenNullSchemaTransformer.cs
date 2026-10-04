using System.Text.Json.Serialization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Stops a property that is left out of the JSON when it is null from being listed as required. Record
/// parameters are required by default, which is wrong for the optional parts of a published sheet.
/// </summary>
public sealed class OmittedWhenNullSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (schema.Required is not { Count: > 0 } required)
        {
            return Task.CompletedTask;
        }

        foreach (var property in context.JsonTypeInfo.Properties)
        {
            var ignore = property.AttributeProvider?
                .GetCustomAttributes(typeof(JsonIgnoreAttribute), inherit: true)
                .OfType<JsonIgnoreAttribute>()
                .FirstOrDefault();

            if (ignore?.Condition is JsonIgnoreCondition.WhenWritingNull or JsonIgnoreCondition.WhenWritingDefault)
            {
                required.Remove(property.Name);
            }
        }

        return Task.CompletedTask;
    }
}
