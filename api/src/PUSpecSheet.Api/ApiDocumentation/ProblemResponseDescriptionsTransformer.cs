using System.Globalization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Describes the problem details responses that an action's own comments don't: an action that says what
/// its 409 means keeps its words, and the rest get the general explanation in place of "Conflict".
/// </summary>
public sealed class ProblemResponseDescriptionsTransformer : IOpenApiOperationTransformer
{
    public const string ContentType = "application/problem+json";

    private static readonly Dictionary<int, string> Defaults = new()
    {
        [StatusCodes.Status400BadRequest] = "The request isn't valid. `detail` or `errors` says why.",
        [StatusCodes.Status403Forbidden] = "The current user lacks the permission this needs. `detail` names it.",
        [StatusCodes.Status404NotFound] = "Nothing exists at this address, or something the request refers to doesn't exist.",
        [StatusCodes.Status409Conflict] =
            "The change conflicts with the current state: the item is being edited by someone else, is in use, "
            + "or a limit would be broken. `detail` says which.",
    };

    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        foreach (var (status, text) in Defaults)
        {
            var key = status.ToString(CultureInfo.InvariantCulture);
            if (operation.Responses is null || !operation.Responses.TryGetValue(key, out var response))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(response.Description) || response.Description == ReasonPhrases.GetReasonPhrase(status))
            {
                response.Description = text;
            }
        }

        return Task.CompletedTask;
    }
}
