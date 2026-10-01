using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.SheetTypes;
using PUSpecSheet.Application.Templates;

namespace PUSpecSheet.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    /// <summary>Registers the application services. Each feature adds its services here.</summary>
    public static IServiceCollection AddPuSpecSheetApplication(this IServiceCollection services)
    {
        services.AddScoped<ISheetTypeService, SheetTypeService>();
        services.AddScoped<IPhaseService, PhaseService>();

        services.AddScoped<ICellTypeService, CellTypeService>();

        services.AddScoped<TableTemplateReader>();
        services.AddScoped<TemplateVersionGuard>();
        services.AddScoped<ITableTemplateService, TableTemplateService>();
        services.AddScoped<ITemplateSectionService, TemplateSectionService>();
        services.AddScoped<ITemplateRowService, TemplateRowService>();
        services.AddScoped<ITemplateCellService, TemplateCellService>();
        services.AddScoped<ITemplateCellOverrideService, TemplateCellOverrideService>();
        services.AddScoped<ITemplateColumnBlockService, TemplateColumnBlockService>();

        services.AddPuSpecSheetSheets();

        return services;
    }
}
