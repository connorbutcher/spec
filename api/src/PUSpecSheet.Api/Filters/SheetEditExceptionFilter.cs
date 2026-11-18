using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PUSpecSheet.Api.DAL.Processors.Drafts.Locks;

namespace PUSpecSheet.Api.Filters;

/// <summary>
/// Turns a refused edit into a response the screens can show: 409 with the reason when an item is checked out to someone
/// else or the edit isn't allowed in the sheet's state, 404 when something named isn't there.
/// </summary>
public class SheetEditExceptionFilter : IExceptionFilter
{
    private readonly ILogger<SheetEditExceptionFilter> _logger;

    public SheetEditExceptionFilter(ILogger<SheetEditExceptionFilter> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var status = context.Exception switch
        {
            SheetItemLockedException => StatusCodes.Status409Conflict,
            InvalidOperationException => StatusCodes.Status409Conflict,
            KeyNotFoundException => StatusCodes.Status404NotFound,
            _ => (int?)null
        };
        if (status is null)
        {
            return;
        }

        _logger.LogWarning("Refused {Method} {Path}: {Reason}", context.HttpContext.Request.Method, context.HttpContext.Request.Path, context.Exception.Message);

        context.Result = new ObjectResult(new ProblemDetails
        {
            Status = status,
            Title = status == StatusCodes.Status404NotFound ? "Not found" : "Not allowed",
            Detail = context.Exception.Message
        })
        {
            StatusCode = status
        };
        context.ExceptionHandled = true;
    }
}
