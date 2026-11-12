using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.Approvals;
using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Application.Published;
using PUSpecSheet.Application.Seeding;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.SheetTypes;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Application.Templates.Cells;
using PUSpecSheet.Application.Templates.ColumnBlocks;
using PUSpecSheet.Application.Templates.Rows;
using PUSpecSheet.Application.Templates.Sections;
using PUSpecSheet.Application.Users;

namespace PUSpecSheet.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddPuSpecSheetApplication(this IServiceCollection services)
    {
        services.AddScoped<ISheetTypeService, SheetTypeService>();
        services.AddScoped<IPhaseService, PhaseService>();
        services.AddScoped<IUserAccessService, UserAccessService>();
        services.AddScoped<BackgroundUser>();

        services.AddScoped<ICellTypeService, CellTypeService>();

        services.AddScoped<TableTemplateReader>();
        services.AddScoped<TemplateVersionGuard>();
        services.AddScoped<ITableTemplateService, TableTemplateService>();
        services.AddScoped<ITemplateSectionService, TemplateSectionService>();
        services.AddScoped<ITemplateRowService, TemplateRowService>();
        services.AddScoped<ITemplateCellService, TemplateCellService>();
        services.AddScoped<ITemplateCellOverrideService, TemplateCellOverrideService>();
        services.AddScoped<ITemplateCellLookupKeyService, TemplateCellLookupKeyService>();
        services.AddScoped<ITemplateColumnBlockService, TemplateColumnBlockService>();

        services.AddPuSpecSheetSheets();
        services.AddPuSpecSheetApprovals();
        services.AddPuSpecSheetPublished();
        services.AddPuSpecSheetSeeding();

        return services;
    }
}
