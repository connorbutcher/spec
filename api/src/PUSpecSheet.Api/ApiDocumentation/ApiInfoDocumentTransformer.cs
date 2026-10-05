using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Sets the document's title and introduction, and lists the tags in group order with their
/// descriptions, which is the order Swagger UI shows them in. The groups also go out as
/// <c>x-tagGroups</c> for tools that draw group headings.
/// </summary>
public sealed class ApiInfoDocumentTransformer : IOpenApiDocumentTransformer
{
    private const string Description =
        """
        Engine specification sheets: sheets of template-driven tables that group limits for a phase, such as
        Specification, PFKs and Parts.

        ## Which endpoints to use

        - **Another application reading limits** uses only *Published sheets* (`/api/published`). It is read-only,
          returns values only, never returns drafts, and is built for caching.
        - **The PU Spec Sheet UI** uses everything else: sheets and their drafts, templates and reference data.

        ## Identifiers

        The editing endpoints use integer ids. Sheets, tables, sections, column blocks, rows and cells also have
        a public identifier (a GUID) that is the same in every version of a sheet; the published endpoints use
        only those.

        ## Drafts and versions

        A change to a sheet is a draft that only its author sees. Changing a row, section or table locks it to
        that person until they publish or discard. Publishing turns all of a person's drafts on a sheet into its
        next version, and a published version never changes.

        ## Errors

        Errors are returned as [problem details](https://www.rfc-editor.org/rfc/rfc9457) (`application/problem+json`),
        with the reason in `detail`: `400` for a request that isn't valid, `404` when something doesn't exist and
        `409` when a change conflicts with the current state, such as an item locked by someone else.

        ## Signing in

        There is no sign-in yet. Every request runs as the seeded developer user.
        """;

    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        document.Info = new OpenApiInfo
        {
            Title = "PU Spec Sheet API",
            Version = "v1",
            Description = Description,
        };

        document.Tags = ApiTagCatalog.Groups
            .SelectMany(group => group.Tags)
            .Select(tag => new OpenApiTag { Name = tag.Name, Description = tag.Description })
            .ToHashSet();

        var groups = new JsonArray();
        foreach (var group in ApiTagCatalog.Groups)
        {
            var names = new JsonArray();
            foreach (var tag in group.Tags)
            {
                names.Add(tag.Name);
            }

            groups.Add(new JsonObject { ["name"] = group.Name, ["tags"] = names });
        }

        document.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        document.Extensions["x-tagGroups"] = new JsonNodeExtension(groups);

        return Task.CompletedTask;
    }
}
