using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Application.Published;

public static class PublishedServiceCollectionExtensions
{
    public static IServiceCollection AddPuSpecSheetPublished(this IServiceCollection services)
    {
        services.AddSingleton<PublishedSheetCache>();
        services.AddScoped<PublishedVersionResolver>();
        services.AddScoped<IPublishedSheetQueryService, PublishedSheetQueryService>();
        services.AddSingleton<PublishedLookupCache>();
        services.AddScoped<IPublishedLookupService, PublishedLookupService>();
        services.AddSingleton<PublishedRowsCache>();
        services.AddScoped<IPublishedRowsService, PublishedRowsService>();

        return services;
    }
}
