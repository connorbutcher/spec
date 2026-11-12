using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Editing;
using PUSpecSheet.Application.Sheets.History;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Application.Sheets.Publishing;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Values;

namespace PUSpecSheet.Application.Sheets;

public static class SheetServiceCollectionExtensions
{
    /// <summary>Registers the services that read and change sheets, their tables, sections and rows.</summary>
    public static IServiceCollection AddPuSpecSheetSheets(this IServiceCollection services)
    {
        services.AddSingleton<SheetChangeHistoryCache>();
        services.AddScoped<SheetChangeHistoryLoader>();
        services.AddScoped<SheetSnapshotLoader>();
        services.AddScoped<SheetReader>();
        services.AddScoped<SheetInstantiator>();
        services.AddScoped<ISheetCellFiller, SheetCellFiller>();
        services.AddScoped<ISheetItemsService, SheetItemsService>();
        services.AddScoped<LiveSectionQuery>();
        services.AddScoped<TableDrafts>();
        services.AddScoped<SectionDrafts>();
        services.AddScoped<RowDrafts>();
        services.AddScoped<IRemovalInProgressGuard, RemovalInProgressGuard>();
        services.AddScoped<RemovalContentsGuard>();
        services.AddScoped<RowDraftStarter>();
        services.AddScoped<NewRowSettings>();
        services.AddScoped<LinkedDropdownValueRule>();
        services.AddScoped<ColumnBlockDrafts>();
        services.AddScoped<DraftSweeper>();
        services.AddScoped<SheetDraftSetLoader>();
        services.AddScoped<SheetPublisher>();

        services.AddScoped<ISheetsService, SheetsService>();
        services.AddScoped<ISheetTablesService, SheetTablesService>();
        services.AddScoped<ISheetSectionsService, SheetSectionsService>();
        services.AddScoped<ISheetRowsService, SheetRowsService>();
        services.AddScoped<ISheetCellSettingsService, SheetCellSettingsService>();
        services.AddScoped<ISheetColumnBlocksService, SheetColumnBlocksService>();

        return services;
    }
}
