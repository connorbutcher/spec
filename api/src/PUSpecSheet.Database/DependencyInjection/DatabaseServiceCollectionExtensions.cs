using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace PUSpecSheet.Database.DependencyInjection;

public static class DatabaseServiceCollectionExtensions
{
    /// <summary>Registers the <see cref="PuSpecSheetDbContext"/> against SQL Server, with its migrations in this assembly.</summary>
    public static IServiceCollection AddPuSpecSheetDatabase(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<PuSpecSheetDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(PuSpecSheetDbContext).Assembly.GetName().Name);
            });

            // The connection string turns MARS on, which stops EF using savepoints inside a transaction. Every
            // transaction here is rolled back as a whole when a save fails, so the warning isn't needed.
            options.ConfigureWarnings(warnings => warnings.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
        });

        return services;
    }
}
