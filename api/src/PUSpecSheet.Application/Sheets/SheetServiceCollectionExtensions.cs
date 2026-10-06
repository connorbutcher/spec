using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.Sheets.Demo;

namespace PUSpecSheet.Application.Sheets;

public static class SheetServiceCollectionExtensions
{
    /// <summary>Registers the services that read and change sheets, their tables, sections and rows.</summary>
    public static IServiceCollection AddPuSpecSheetSheets(this IServiceCollection services)
    {
        services.AddScoped<RowValueStore>();
        services.AddSingleton<SheetChangeHistoryCache>();
        services.AddScoped<SheetChangeHistoryLoader>();
        services.AddScoped<SheetSnapshotLoader>();
        services.AddScoped<SheetReader>();
        services.AddScoped<ISheetItemLocator, SheetItemLocator>();
        services.AddScoped<SheetInstantiator>();
        services.AddScoped<LiveSectionQuery>();
        services.AddScoped<TableDrafts>();
        services.AddScoped<SectionDrafts>();
        services.AddScoped<RowDrafts>();
        services.AddScoped<ColumnBlockDrafts>();
        services.AddScoped<DraftSweeper>();
        services.AddScoped<ISheetCellFiller, SheetCellFiller>();

        services.AddScoped<ISheetService, SheetService>();
        services.AddScoped<ISheetTableService, SheetTableService>();
        services.AddScoped<ISheetSectionService, SheetSectionService>();
        services.AddScoped<ISheetRowService, SheetRowService>();
        services.AddScoped<ISheetColumnBlockService, SheetColumnBlockService>();

        services.AddScoped<DemoLimitsTemplateSeeder>();
        services.AddScoped<DemoLimitsSheetSeeder>();
        services.AddScoped<DemoPartsGridTemplateSeeder>();
        services.AddScoped<DemoPartsGridSheetSeeder>();
        services.AddScoped<DemoDataSeeder>();

        return services;
    }
}
