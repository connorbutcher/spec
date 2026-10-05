using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Tidies the descriptions taken from XML comments. A <c>&lt;see cref&gt;</c> to a property arrives as its
/// C# signature, such as <c>Guid? PublishedCellDto.Column</c>; this turns it into the name the JSON uses,
/// <c>column</c>.
/// </summary>
public sealed partial class MemberReferenceSchemaTransformer : IOpenApiSchemaTransformer
{
    public Task TransformAsync(OpenApiSchema schema, OpenApiSchemaTransformerContext context, CancellationToken cancellationToken)
    {
        if (!string.IsNullOrEmpty(schema.Description))
        {
            schema.Description = MemberReference().Replace(schema.Description, JsonName);
        }

        return Task.CompletedTask;
    }

    private static string JsonName(Match match)
    {
        return $"`{JsonNamingPolicy.CamelCase.ConvertName(match.Groups["member"].Value)}`";
    }

    /// <summary>A type, a space, then <c>Owner.Member</c>. The type may be generic with spaces inside it.</summary>
    [GeneratedRegex(@"(?:[\w.]+(?:<|&lt;).*?(?:>|&gt;)\??|[\w.?\[\]]+) [A-Z]\w*\.(?<member>[A-Z]\w*)\b")]
    private static partial Regex MemberReference();
}
