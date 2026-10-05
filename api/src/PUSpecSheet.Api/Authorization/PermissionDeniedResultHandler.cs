using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Mvc;

namespace PUSpecSheet.Api.Authorization;

/// <summary>
/// Answers a request that lacks a permission with a 403 ProblemDetails naming the permission, like the
/// API's other errors. Every other authorization outcome is handled as usual.
/// </summary>
public sealed class PermissionDeniedResultHandler(IProblemDetailsService problemDetails) : IAuthorizationMiddlewareResultHandler
{
    private readonly AuthorizationMiddlewareResultHandler fallback = new();

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        var missing = authorizeResult.AuthorizationFailure?.FailedRequirements
            .OfType<PermissionRequirement>()
            .FirstOrDefault();

        if (!authorizeResult.Forbidden || missing is null)
        {
            await fallback.HandleAsync(next, context, policy, authorizeResult);
            return;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new ProblemDetails
            {
                Status = StatusCodes.Status403Forbidden,
                Detail = $"You need the \"{missing.Permission}\" permission to do this.",
            },
        });
    }
}
