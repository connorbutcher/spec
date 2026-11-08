using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PUSpecSheet.Application.Sheets.Collaboration;
using PUSpecSheet.Contracts.Sheets;

namespace PUSpecSheet.Api.Collaboration;

/// <summary>
/// Every action that reads or changes a sheet answers with a <see cref="SheetDto"/>, or with some of its
/// rows in a <see cref="SheetRowsChangeDto"/> that keeps the sheet they were taken from. This filter looks
/// at that sheet once the answer has been sent and, after a change, tells the other people who have the sheet open,
/// so no controller or service has to remember to. The caller names its own live connection in the
/// <see cref="ConnectionHeader"/> header so it isn't told about its own change.
/// <para>
/// It runs for every action, so it is registered once for the application and does nothing but a type
/// check for an answer that isn't a live sheet.
/// </para>
/// </summary>
public sealed partial class SheetChangedFilter(SheetChangeAnnouncer announcer, ILogger<SheetChangedFilter> logger) : IAsyncResultFilter
{
    public const string ConnectionHeader = "X-Sheet-Connection";

    public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
    {
        // Taken before the answer is sent, while the result is still the one the action returned.
        var sheet = LiveSheetOf(context.Result);
        await next();

        if (sheet is null)
        {
            return;
        }

        var request = context.HttpContext.Request;
        if (HttpMethods.IsGet(request.Method))
        {
            announcer.Observe(sheet);
            return;
        }

        // The change is saved and answered; telling the others must not fail it or stop with the request.
        try
        {
            var settler = context.HttpContext.RequestServices.GetRequiredService<IRowTakeoverSettler>();
            await settler.CloseBlockedAsync(sheet.Id, CancellationToken.None);
            await announcer.AnnounceAsync(sheet, request.Headers[ConnectionHeader].FirstOrDefault(), CancellationToken.None);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogAnnounceFailed(logger, exception, sheet.Id);
        }
    }

    private static SheetDto? LiveSheetOf(IActionResult result)
    {
        var sheet = result switch
        {
            ObjectResult { Value: SheetDto whole } => whole,
            ObjectResult { Value: SheetRowsChangeDto { Sheet: { } source } } => source,
            _ => null,
        };
        return sheet is { IsLive: true } ? sheet : null;
    }

    [LoggerMessage(Level = LogLevel.Warning, Message = "Couldn't tell the people on sheet {SheetId} that it changed.")]
    private static partial void LogAnnounceFailed(ILogger logger, Exception exception, int sheetId);
}
