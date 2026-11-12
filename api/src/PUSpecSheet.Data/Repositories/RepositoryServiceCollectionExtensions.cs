using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.Approvals;
using PUSpecSheet.Application.Persistence.CellTypes;
using PUSpecSheet.Application.Persistence.Phases;
using PUSpecSheet.Application.Persistence.Published;
using PUSpecSheet.Application.Persistence.Seeding;
using PUSpecSheet.Application.Persistence.Sheets;
using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Application.Sheets.Layout;
using PUSpecSheet.Application.Sheets.Reading;
using PUSpecSheet.Data.Repositories.Approvals;
using PUSpecSheet.Data.Repositories.CellTypes;
using PUSpecSheet.Data.Repositories.Phases;
using PUSpecSheet.Data.Repositories.Published;
using PUSpecSheet.Data.Repositories.Seeding;
using PUSpecSheet.Data.Repositories.Sheets;
using PUSpecSheet.Data.Repositories.Sheets.Revisions;
using PUSpecSheet.Data.Repositories.Sheets.Values;
using PUSpecSheet.Data.Repositories.SheetTypes;
using PUSpecSheet.Data.Repositories.Templates;
using PUSpecSheet.Data.Repositories.Users;
using PUSpecSheet.Data.UnitOfWork;

namespace PUSpecSheet.Data.Repositories;

public static class RepositoryServiceCollectionExtensions
{
    /// <summary>Registers the unit of work and every repository, grouped by area.</summary>
    public static IServiceCollection AddPuSpecSheetRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork, EfUnitOfWork>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ISheetTypeRepository, SheetTypeRepository>();
        services.AddScoped<IPhaseRepository, PhaseRepository>();
        services.AddScoped<ICellTypeRepository, CellTypeRepository>();

        services.AddScoped<ITableTemplateRepository, TableTemplateRepository>();
        services.AddScoped<ITemplateSectionRepository, TemplateSectionRepository>();
        services.AddScoped<ITemplateRowRepository, TemplateRowRepository>();
        services.AddScoped<ITemplateCellRepository, TemplateCellRepository>();
        services.AddScoped<ITemplateColumnBlockRepository, TemplateColumnBlockRepository>();

        services.AddScoped<ISheetRepository, SheetRepository>();
        services.AddScoped<ISheetTableRepository, SheetTableRepository>();
        services.AddScoped<ISheetSectionRepository, SheetSectionRepository>();
        services.AddScoped<ISheetRowRepository, SheetRowRepository>();
        services.AddScoped<ISheetCellRepository, SheetCellRepository>();
        services.AddScoped<ISheetColumnBlockRepository, SheetColumnBlockRepository>();
        services.AddScoped<ISheetTableRevisionRepository, SheetTableRevisionRepository>();
        services.AddScoped<ISheetSectionRevisionRepository, SheetSectionRevisionRepository>();
        services.AddScoped<ISheetRowRevisionRepository, SheetRowRevisionRepository>();
        services.AddScoped<ISheetColumnBlockRevisionRepository, SheetColumnBlockRevisionRepository>();
        services.AddScoped<IRowValueRepository, RowValueRepository>();
        services.AddScoped<ISheetItemLocator, SheetItemLocator>();
        services.AddScoped<ISheetCellFiller, SheetCellFiller>();

        services.AddScoped<ISheetSubmissionRepository, SheetSubmissionRepository>();

        services.AddScoped<ISampleDataRepository, SampleDataRepository>();

        services.AddScoped<IPublishedSheetRepository, PublishedSheetRepository>();
        services.AddScoped<IPublishedStructureRepository, PublishedStructureRepository>();
        services.AddScoped<IPublishedCellRepository, PublishedCellRepository>();
        services.AddScoped<IPublishedValueRepository, PublishedValueRepository>();
        services.AddScoped<IPublishedLookupHitRepository, PublishedLookupHitRepository>();
        services.AddScoped<IPublishedLookupCellRepository, PublishedLookupCellRepository>();
        services.AddScoped<IPublishedKindValueRepository, PublishedKindValueRepository>();

        return services;
    }
}
