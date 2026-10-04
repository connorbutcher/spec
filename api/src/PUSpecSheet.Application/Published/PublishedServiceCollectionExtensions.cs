using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Application.Published;

public static class PublishedServiceCollectionExtensions
{
    /// <summary>Registers the read-only services other applications use to query published sheets.</summary>
    public static IServiceCollection AddPuSpecSheetPublished(this IServiceCollection services)
    {
        services.AddSingleton<PublishedSheetCache>();
        services.AddScoped<PublishedVersionResolver>();
        services.AddScoped<PublishedStructureReader>();
        services.AddScoped<PublishedCellReader>();
        services.AddScoped<PublishedValueReader>();
        services.AddScoped<IPublishedSheetQueryService, PublishedSheetQueryService>();

        return services;
    }
}
