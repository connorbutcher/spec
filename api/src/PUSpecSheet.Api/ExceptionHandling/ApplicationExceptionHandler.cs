using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Common;

namespace PUSpecSheet.Api.ExceptionHandling;

/// <summary>
/// Turns application exceptions into ProblemDetails responses with the status they stand for, so
/// controllers only carry their happy path. Anything unrecognised falls through to the default 500.
/// </summary>
public sealed class ApplicationExceptionHandler(IProblemDetailsService problemDetails) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var status = StatusFor(exception);
        if (status is null)
        {
            return false;
        }

        httpContext.Response.StatusCode = status.Value;
        return await problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Detail = exception.Message,
            },
        });
    }

    private static int? StatusFor(Exception exception)
    {
        return exception switch
        {
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            InvalidRequestException => StatusCodes.Status400BadRequest,
            _ => null,
        };
    }
}
