using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.Approvals;
using PUSpecSheet.Application.CellTypes;
using PUSpecSheet.Application.Phases;
using PUSpecSheet.Application.Published.Lookups;
using PUSpecSheet.Application.Published.Rows;
using PUSpecSheet.Application.Published.Sheets;
using PUSpecSheet.Application.Published.Versions;
using PUSpecSheet.Application.Seeding;
using PUSpecSheet.Application.Sheets;
using PUSpecSheet.Application.Sheets.Catalog;
using PUSpecSheet.Application.Sheets.Drafts;
using PUSpecSheet.Application.Sheets.Editing;
using PUSpecSheet.Application.Sheets.History;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Linking;
using PUSpecSheet.Application.Sheets.Publishing;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Application.Sheets.Removal;
using PUSpecSheet.Application.Sheets.Values;
using PUSpecSheet.Application.SheetTypes;
using PUSpecSheet.Application.Templates;
using PUSpecSheet.Application.Templates.Cells;
using PUSpecSheet.Application.Templates.ColumnBlocks;
using PUSpecSheet.Application.Templates.Rows;
using PUSpecSheet.Application.Templates.Sections;
using PUSpecSheet.Application.Users;

namespace PUSpecSheet.Application.DependencyInjection;

/// <summary>
/// Every Application registration, by area. Each controller's service is registered against its interface; the
/// classes it works through are registered as themselves. Collaboration is registered by the host, which
/// supplies its options (see <c>AddPuSpecSheetCollaboration</c>).
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddPuSpecSheetApplication(this IServiceCollection services)
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
        services.AddScoped<ISheetItemsService, SheetItemsService>();
        services.AddScoped<ISheetTablesService, SheetTablesService>();
        services.AddScoped<ISheetSectionsService, SheetSectionsService>();
        services.AddScoped<ISheetRowsService, SheetRowsService>();
        services.AddScoped<ISheetColumnBlocksService, SheetColumnBlocksService>();
    }

    /// <summary>Publishing with approval: submitting, deciding and listing submissions.</summary>
    private static void AddApprovals(IServiceCollection services)
    {
        services.AddScoped<ApproverDirectory>();
        services.AddScoped<SheetSubmitter>();
        services.AddScoped<SubmissionDecider>();
        services.AddScoped<SubmissionQueries>();
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

    /// <summary>The development-only showcase sample data and the parts it is built from.</summary>
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
