using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using PUSpecSheet.Api.Published;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Describes the query string of a published sheet read. Its parameters come from the properties of
/// <see cref="PublishedSheetQueryOptions"/>, whose comments the document generator doesn't read.
/// </summary>
public sealed class PublishedSelectionParametersTransformer : IOpenApiOperationTransformer
{
    private const string Combined = " Combined with the other lists; with none of them the whole sheet is returned.";

    private static readonly Dictionary<string, string> Descriptions = new(StringComparer.OrdinalIgnoreCase)
    {
        ["tables"] = "Comma-separated public identifiers of tables to read, each with everything beneath it." + Combined,
        ["sections"] = "Comma-separated public identifiers of sections to read, each with its rows and sub-sections." + Combined,
        ["rows"] = "Comma-separated public identifiers of rows to read." + Combined,
        ["cells"] = "Comma-separated public identifiers of cells to read." + Combined,
        ["shape"] = "`Tree` (the default) nests tables, sections, rows and cells in display order. `Flat` gives one map of cell identifier to value, for a caller that knows the cells it wants.",
        ["include"] = "Set to `labels` to add table titles, section names and cell captions. Nothing else can be included.",
    };

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        var takesSelection = context.Description.ParameterDescriptions
            .Any(parameter => parameter.ModelMetadata?.ContainerType == typeof(PublishedSheetQueryOptions));
        if (!takesSelection)
        {
            return Task.CompletedTask;
        }

        foreach (var parameter in operation.Parameters?.OfType<OpenApiParameter>() ?? [])
        {
            if (parameter.In == ParameterLocation.Query && parameter.Name is { } name && Descriptions.TryGetValue(name, out var description))
            {
                parameter.Description = description;
            }
        }

        return Task.CompletedTask;
    }
}
