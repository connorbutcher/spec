using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace PUSpecSheet.Application.Sheets.Collaboration;

public static class CollaborationServiceCollectionExtensions
{
    /// <summary>
    /// Registers presence, live change announcements and row takeovers. The host supplies the
    /// <see cref="ISheetLiveNotifier"/> that reaches the connected clients.
    /// </summary>
    public static IServiceCollection AddPuSpecSheetCollaboration(this IServiceCollection services, RowTakeoverOptions takeoverOptions)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(takeoverOptions);
        services.AddSingleton<SheetPresenceTracker>();
        services.AddSingleton<RowTakeoverStore>();
        services.AddSingleton<SheetChangeAnnouncer>();

        services.AddScoped<ISheetPresenceService, SheetPresenceService>();
        services.AddScoped<IRowTakeoverService, RowTakeoverService>();

        return services;
    }
}
