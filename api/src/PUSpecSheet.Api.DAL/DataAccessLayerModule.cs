using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PUSpecSheet.Api.Common.Context;
using PUSpecSheet.Api.Common.Redis;
using PUSpecSheet.Api.DAL.Caching;
using PUSpecSheet.Api.DAL.Context;
using PUSpecSheet.Api.DAL.DataAccess.Admin;
using PUSpecSheet.Api.DAL.DataAccess.Query;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets;
using PUSpecSheet.Api.DAL.DataAccess.SpecSheets.Reads;
using PUSpecSheet.Api.DAL.DataAccess.Template;
using PUSpecSheet.Api.DAL.DataAccess.Types;
using PUSpecSheet.Api.DAL.Query.Resolvers.Cells;
using PUSpecSheet.Api.DAL.Query.Resolvers.Tables;
using PUSpecSheet.Api.Database;
using StackExchange.Redis;

namespace PUSpecSheet.Api.DAL;

/// <summary>
/// The data access layer module.
/// </summary>
public static class DataAccessLayerModule
{
    /// <summary>
    /// Adds data access layer declarations to the builder.
    /// </summary>
    /// <param name="builder">The api builder.</param>
    public static void AddDataAccessLayerModule(this IHostApplicationBuilder builder)
    {
        builder.Services.AddDbContextFactory<PUSpecSheetDbContext>(options =>
        {
            options.EnableDetailedErrors();
            options.UseSqlServer(builder.Configuration.GetConnectionString("PUSpecSheet"));
        });

        var redisConfiguration = RedisConfigurationOptions.GetConfigurationOptions(builder.Configuration.GetRequiredSection("Redis"));
        builder.Services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(sp =>
        {
            return ConnectionMultiplexer.Connect(redisConfiguration);
        });

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.ConfigurationOptions = redisConfiguration;
        });

        builder.Services.AddScoped<IUserContext, UserContext>();
        builder.Services.AddScoped<IAdminDataAccessService, AdminDataAccessService>();
        builder.Services.AddScoped<ISpecSheetDataAccessService, SpecSheetDataAccessService>();
        builder.Services.AddScoped<ITemplateDataAccessService, TemplateDataAccessService>();
        builder.Services.AddScoped<ITypeDataAccessService, TypeDataAccessService>();
        builder.Services.AddScoped<IQueryDataAccessService, QueryDataAccessService>();
        builder.Services.AddScoped<ISpecSheetCache, SpecSheetCache>();

        RegisterReaders(builder);
        RegisterResolvers(builder.Services);
    }

    private static void RegisterReaders(IHostApplicationBuilder builder)
    {
        builder.Services.Configure<SheetReadOptions>(builder.Configuration.GetSection(SheetReadOptions.SectionName));
        builder.Services.AddScoped<SpecSheetCopyReader>();
        builder.Services.AddScoped<SpecSheetRevisionReader>();
        builder.Services.AddScoped<ISpecSheetReader, SpecSheetReader>();
    }

    private static void RegisterResolvers(IServiceCollection services)
    {
        services.AddScoped<LinkedTableResolver>();
        services.AddScoped<LinkedSpecSheetLookupCellResolver>();
    }
}
