using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.DataAccess.Approvals;
using PUSpecSheet.DataAccess.CellTypes;
using PUSpecSheet.DataAccess.Phases;
using PUSpecSheet.DataAccess.Published.Lookups;
using PUSpecSheet.DataAccess.Published.Rows;
using PUSpecSheet.DataAccess.Published.Sheets;
using PUSpecSheet.DataAccess.Published.Versions;
using PUSpecSheet.DataAccess.Seeding;
using PUSpecSheet.DataAccess.Seeding.ReferenceData;
using PUSpecSheet.DataAccess.Seeding.Sheets;
using PUSpecSheet.DataAccess.Seeding.Templates;
using PUSpecSheet.DataAccess.Sheets;
using PUSpecSheet.DataAccess.Sheets.Catalog;
using PUSpecSheet.DataAccess.Sheets.Drafts;
using PUSpecSheet.DataAccess.Sheets.Editing;
using PUSpecSheet.DataAccess.Sheets.History;
using PUSpecSheet.DataAccess.Sheets.Layout;
using PUSpecSheet.DataAccess.Sheets.Linking;
using PUSpecSheet.DataAccess.Sheets.Publishing;
using PUSpecSheet.DataAccess.Sheets.Reading;
using PUSpecSheet.DataAccess.Sheets.Removal;
using PUSpecSheet.DataAccess.Sheets.Values;
using PUSpecSheet.DataAccess.SheetTypes;
using PUSpecSheet.DataAccess.Templates;
using PUSpecSheet.DataAccess.Templates.Cells;
using PUSpecSheet.DataAccess.Templates.ColumnBlocks;
using PUSpecSheet.DataAccess.Templates.Rows;
using PUSpecSheet.DataAccess.Templates.Sections;
using PUSpecSheet.DataAccess.Users;

namespace PUSpecSheet.DataAccess.DependencyInjection;

public static class DataAccessServiceCollectionExtensions
{
    public static IServiceCollection AddPuSpecSheetDataAccess(this IServiceCollection services)
    {
        AddReferenceData(services);
        AddTemplates(services);
        AddSheets(services);
        AddApprovals(services);
        AddPublished(services);
        AddSeeding(services);

        return services;
    }

    private static void AddReferenceData(IServiceCollection services)
    {
        services.AddScoped<IPhaseDataService, PhaseDataService>();
        services.AddScoped<ISheetTypeDataService, SheetTypeDataService>();
        services.AddScoped<ICellTypeDataService, CellTypeDataService>();
        services.AddScoped<ISheetTypesService, SheetTypesService>();
        services.AddScoped<IPhasesService, PhasesService>();
        services.AddScoped<ICellTypesService, CellTypesService>();
        services.AddScoped<IUserAccessService, UserAccessService>();
        services.AddScoped<BackgroundUser>();
    }

    private static void AddTemplates(IServiceCollection services)
    {
        services.AddScoped<TableTemplateReader>();
        services.AddScoped<TemplateVersionGuard>();
        services.AddScoped<ITableTemplatesService, TableTemplatesService>();
        services.AddScoped<ITemplateSectionsService, TemplateSectionsService>();
        services.AddScoped<ITemplateRowsService, TemplateRowsService>();
        services.AddScoped<ITemplateCellsService, TemplateCellsService>();
        services.AddScoped<ITemplateCellOverridesService, TemplateCellOverridesService>();
        services.AddScoped<ITemplateCellLookupKeysService, TemplateCellLookupKeysService>();
        services.AddScoped<ITemplateColumnBlocksService, TemplateColumnBlocksService>();
    }

    private static void AddSheets(IServiceCollection services)
    {
        services.AddSingleton<SheetChangeHistoryCache>();
        services.AddScoped<SheetChangeHistoryLoader>();
        services.AddScoped<SheetSnapshotLoader>();
        services.AddScoped<SheetReader>();
        services.AddScoped<SheetInstantiator>();
        services.AddScoped<ISheetCellFiller, SheetCellFiller>();
        services.AddScoped<LiveSectionQuery>();
        services.AddScoped<TableDrafts>();
        services.AddScoped<SectionDrafts>();
        services.AddScoped<RowDrafts>();
        services.AddScoped<ColumnBlockDrafts>();
        services.AddScoped<IRemovalInProgressGuard, RemovalInProgressGuard>();
        services.AddScoped<RemovalContentsGuard>();
        services.AddScoped<RowDraftStarter>();
        services.AddScoped<NewRowSettings>();
        services.AddScoped<LinkedDropdownValueRule>();
        services.AddScoped<DraftSweeper>();
        services.AddScoped<SheetDraftSetLoader>();
        services.AddScoped<SheetPublisher>();
        services.AddScoped<ISheetCellSettingsService, SheetCellSettingsService>();

        services.AddScoped<SheetCopier>();
        services.AddScoped<SheetCreator>();
        services.AddScoped<ISheetsService, SheetsService>();
        services.AddScoped<ISheetTablesService, SheetTablesService>();
        services.AddScoped<ISheetSectionsService, SheetSectionsService>();
        services.AddScoped<ISheetRowsService, SheetRowsService>();
        services.AddScoped<ISheetColumnBlocksService, SheetColumnBlocksService>();
    }

    private static void AddApprovals(IServiceCollection services)
    {
        services.AddScoped<ApproverDirectory>();
        services.AddScoped<SheetSubmitter>();
        services.AddScoped<SubmissionDecider>();
        services.AddScoped<SubmissionReader>();
        services.AddScoped<ISubmissionsService, SubmissionsService>();
    }

    private static void AddPublished(IServiceCollection services)
    {
        services.AddSingleton<PublishedSheetCache>();
        services.AddSingleton<PublishedLookupCache>();
        services.AddSingleton<PublishedRowsCache>();
        services.AddScoped<PublishedVersionResolver>();
        services.AddScoped<IPublishedSheetsService, PublishedSheetsService>();
        services.AddScoped<IPublishedLookupService, PublishedLookupService>();
        services.AddScoped<IPublishedRowsService, PublishedRowsService>();
    }

    private static void AddSeeding(IServiceCollection services)
    {
        services.AddScoped<ShowcaseCellTypes>();
        services.AddScoped<ShowcasePhases>();
        services.AddScoped<ShowcaseUsers>();
        services.AddScoped<ShowcaseTemplateWriter>();
        services.AddScoped<SheetEditorFactory>();
        services.AddScoped<SpecificationSheetSeeder>();
        services.AddScoped<PfkSheetSeeder>();
        services.AddScoped<PartsSheetSeeder>();
        services.AddScoped<ShowcaseSheetIdentifiers>();
        services.AddScoped<EngineSpecificationSheetSeeder>();
        services.AddScoped<ShowcaseDataSeeder>();
    }
}
