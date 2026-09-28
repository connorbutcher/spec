using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Data.DependencyInjection;

public static class DataServiceCollectionExtensions
{
    /// <summary>Registers the <see cref="PuSpecSheetDbContext"/> against SQL Server.</summary>
    public static IServiceCollection AddPuSpecSheetData(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PuSpecSheetDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(PuSpecSheetDbContext).Assembly.GetName().Name);
            });
        });

        return services;
    }
}
