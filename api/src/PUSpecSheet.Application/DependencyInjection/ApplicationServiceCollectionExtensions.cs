using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Application.SheetTypes;

namespace PUSpecSheet.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Registers the application services. Each feature adds its services here.</summary>
    public static IServiceCollection AddPuSpecSheetApplication(this IServiceCollection services)
    {
        services.AddScoped<ISheetTypeService, SheetTypeService>();
        services.AddScoped<IPhaseService, PhaseService>();

        return services;
    }
}
