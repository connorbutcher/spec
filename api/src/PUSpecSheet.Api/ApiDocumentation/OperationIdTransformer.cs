using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace PUSpecSheet.Api.ApiDocumentation;

/// <summary>
/// Gives each operation a stable id, <c>Controller_Action</c>, which the documentation uses in its links
/// and client generators use as the method name.
/// </summary>
public sealed class OperationIdTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        if (context.Description.ActionDescriptor is ControllerActionDescriptor action)
        {
            operation.OperationId ??= $"{action.ControllerName}_{action.ActionName}";
        }

        return Task.CompletedTask;
    }
}
