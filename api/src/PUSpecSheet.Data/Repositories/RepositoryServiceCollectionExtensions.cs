using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Application.Persistence;
using PUSpecSheet.Application.Persistence.CellTypes;
using PUSpecSheet.Application.Persistence.Phases;
using PUSpecSheet.Application.Persistence.SheetTypes;
using PUSpecSheet.Application.Persistence.Templates;
using PUSpecSheet.Application.Persistence.Users;
using PUSpecSheet.Data.Repositories.CellTypes;
using PUSpecSheet.Data.Repositories.Phases;
using PUSpecSheet.Data.Repositories.SheetTypes;
using PUSpecSheet.Data.Repositories.Templates;
using PUSpecSheet.Data.Repositories.Users;
using PUSpecSheet.Data.UnitOfWork;

namespace PUSpecSheet.Data.Repositories;

public static class RepositoryServiceCollectionExtensions
{
    /// <summary>Registers the unit of work and every repository, one per area.</summary>
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

        return services;
    }
}
