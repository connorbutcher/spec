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
        services.AddScoped<ISheetTypesService, SheetTypesService>();
        services.AddScoped<IPhasesService, PhasesService>();
        services.AddScoped<IUserAccessService, UserAccessService>();
        services.AddScoped<BackgroundUser>();

        services.AddScoped<ICellTypesService, CellTypesService>();

        services.AddScoped<TableTemplateReader>();
        services.AddScoped<TemplateVersionGuard>();
        services.AddScoped<ITableTemplatesService, TableTemplatesService>();
        services.AddScoped<ITemplateSectionsService, TemplateSectionsService>();
        services.AddScoped<ITemplateRowsService, TemplateRowsService>();
        services.AddScoped<ITemplateCellsService, TemplateCellsService>();
        services.AddScoped<ITemplateCellOverridesService, TemplateCellOverridesService>();
        services.AddScoped<ITemplateCellLookupKeysService, TemplateCellLookupKeysService>();
        services.AddScoped<ITemplateColumnBlocksService, TemplateColumnBlocksService>();

        services.AddPuSpecSheetSheets();
        services.AddPuSpecSheetApprovals();
        services.AddPuSpecSheetPublished();
        services.AddPuSpecSheetSeeding();

        return services;
    }
}
