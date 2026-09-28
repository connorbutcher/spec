using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Registers the application services. Each feature adds its services here.</summary>
    public static IServiceCollection AddPuSpecSheetApplication(this IServiceCollection services)
    {
        return services;
    }
}
