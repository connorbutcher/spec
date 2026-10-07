using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>
/// Every action that changes a sheet answers with the refreshed live sheet. This filter looks at that
/// answer once it has been sent and tells the other people who have the sheet open, so no controller or
/// service has to remember to. The caller names its own live connection in the
/// <see cref="ConnectionHeader"/> header so it isn't told about its own change.
/// </summary>
public sealed partial class SheetChangedFilter(
    SheetChangeAnnouncer announcer,
    IRowTakeoverService takeovers,
    ILogger<SheetChangedFilter> logger) : IAsyncResultFilter
{
    public const string ConnectionHeader = "X-Sheet-Connection";

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        await next();

        var request = context.HttpContext.Request;
        if (context.Result is not ObjectResult { Value: SheetDto { IsLive: true } sheet })
        {
            return;
        }

        if (HttpMethods.IsGet(request.Method))
        {
            announcer.Observe(sheet);
            return;
        }

        // The change is saved and answered; telling the others must not fail it or stop with the request.
        try
        {
            await takeovers.ReleaseSettledAsync(sheet.Id, CancellationToken.None);
            await announcer.AnnounceAsync(sheet, request.Headers[ConnectionHeader].FirstOrDefault(), CancellationToken.None);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogAnnounceFailed(logger, exception, sheet.Id);
        }
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Couldn't tell the people on sheet {SheetId} that it changed.")]
    private static partial void LogAnnounceFailed(ILogger logger, Exception exception, int sheetId);
}
