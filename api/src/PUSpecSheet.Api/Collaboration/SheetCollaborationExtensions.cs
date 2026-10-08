using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using PUSpecSheet.Application.Sheets.Collaboration;

namespace PUSpecSheet.Api.Collaboration;

public static class SheetCollaborationExtensions
{
    /// <summary>Where browsers connect to watch a sheet.</summary>
    public const string HubPath = "/hubs/sheets";

    /// <summary>
    /// Registers multi-user editing: the SignalR hub that tracks who has each sheet open, live
    /// announcements of checkouts and publishes, and takeover requests.
    /// </summary>
    public static IServiceCollection AddPuSpecSheetCollaboration(this IServiceCollection services, IConfiguration configuration)
    {
        var takeoverOptions = configuration.GetSection(RowTakeoverOptions.SectionName).Get<RowTakeoverOptions>() ?? new RowTakeoverOptions();
        services.AddPuSpecSheetCollaboration(takeoverOptions);

        // Enums go to the browser as their names, as they do from the controllers.
        services.AddSignalR()
            .AddJsonProtocol(options => options.PayloadSerializerOptions.Converters.Add(new JsonStringEnumConverter()));
        services.AddSingleton<ISheetLiveNotifier, SignalRSheetLiveNotifier>();
        services.AddHostedService<RowTakeoverExpiryWorker>();

        services.AddSingleton<SheetChangedFilter>();
        services.Configure<MvcOptions>(options => options.Filters.AddService<SheetChangedFilter>());

        return services;
    }

    public static IEndpointRouteBuilder MapPuSpecSheetCollaboration(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHub<SheetHub>(HubPath);
        return endpoints;
    }
}
