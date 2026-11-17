using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Checkouts;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Presence;
using PUSpecSheet.DataAccess.Sheets.Collaboration.Takeovers;

namespace PUSpecSheet.DataAccess.Sheets.Collaboration;

public static class CollaborationServiceCollectionExtensions
{
    public static IServiceCollection AddPuSpecSheetCollaboration(this IServiceCollection services, RowTakeoverOptions takeoverOptions)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddSingleton(takeoverOptions);
        services.AddSingleton<SheetPresenceTracker>();
        services.AddSingleton<RowTakeoverStore>();
        services.AddSingleton<LiveRowCheckoutTracker>();
        services.AddSingleton<SheetChangeAnnouncer>();

        services.AddScoped<ISheetPresenceService, SheetPresenceService>();
        services.AddScoped<IRowCheckouts, RowCheckouts>();
        services.AddScoped<ILiveRowCheckoutService, LiveRowCheckoutService>();
        services.AddScoped<LiveRowCheckoutGuard>();
        services.AddScoped<RowTakeoverCloser>();
        services.AddScoped<IRowTakeoversService, RowTakeoversService>();
        services.AddScoped<IRowTakeoverSettler, RowTakeoverSettler>();

        return services;
    }
}
