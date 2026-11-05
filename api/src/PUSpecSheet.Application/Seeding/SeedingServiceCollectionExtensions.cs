using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Application.Seeding;

public static class SeedingServiceCollectionExtensions
{
    /// <summary>Registers the development-only showcase sample data and the parts it is built from.</summary>
    public static IServiceCollection AddPuSpecSheetSeeding(this IServiceCollection services)
    {
        services.AddScoped<SampleDataReset>();
        services.AddScoped<ShowcaseCellTypes>();
        services.AddScoped<ShowcasePhases>();
        services.AddScoped<ShowcaseUsers>();
        services.AddScoped<ShowcaseTemplateWriter>();
        services.AddScoped<SheetEditorFactory>();
        services.AddScoped<SpecificationSheetSeeder>();
        services.AddScoped<PfkSheetSeeder>();
        services.AddScoped<PartsSheetSeeder>();
        services.AddScoped<ShowcaseDataSeeder>();

        return services;
    }
}
