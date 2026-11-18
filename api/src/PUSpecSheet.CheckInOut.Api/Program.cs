using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;
using PUSpecSheet.Api.Common.Cors;
using PUSpecSheet.Api.Common.Documentation;
using PUSpecSheet.Api.Common.Logging;
using PUSpecSheet.Api.Common.Redis;
using PUSpecSheet.CheckInOut.Api.Hubs;
using PUSpecSheet.CheckInOut.Api.Models;
using PUSpecSheet.CheckInOut.Api.Services.Presence;
using PUSpecSheet.CheckInOut.Api.Services.Users;
// using StackExchange.Redis;

OpenApiInfo apiInfo = new()
{
    Title = "PUSpecSheet.CheckInOut.API",
    Description = "CheckInOut Service for PU Spec Sheet system.",
    Version = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()!.Location).FileVersion
};

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHttpContextAccessor();

builder.AddSerilog();

builder.Services.AddCorsPolicy(builder.Configuration.GetRequiredSection("Cors"));
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.AddControllers();

// Redis is disabled for local running: the caches are held in memory and SignalR runs without a backplane.
// var redisConfiguration = RedisConfigurationOptions.GetConfigurationOptions(builder.Configuration.GetRequiredSection("Redis"));
// builder.Services.AddSingleton<IConnectionMultiplexer, ConnectionMultiplexer>(sp =>
// {
//     return ConnectionMultiplexer.Connect(redisConfiguration);
// });
//
// builder.Services.AddStackExchangeRedisCache(options =>
// {
//     options.ConfigurationOptions = redisConfiguration;
// });

builder.Services.AddSignalR(x =>
{
    x.KeepAliveInterval = TimeSpan.FromSeconds(5);
    x.ClientTimeoutInterval = TimeSpan.FromSeconds(10);
    x.EnableDetailedErrors = true;
});
// .AddStackExchangeRedis(o => o.Configuration = redisConfiguration);

builder.AddDocumentation(apiInfo);

// builder.Services.AddKeyedSingleton<ICacheProvider<ConnectedUser>, DistributedCacheProvider<ConnectedUser>>(ConnectedUserService.ConnectedUsersCacheName);
// builder.Services.AddKeyedSingleton<ICacheProvider<ConnectedUser>, DistributedCacheProvider<ConnectedUser>>(SheetPresenceService.SheetPresenceCacheName);
builder.Services.AddKeyedSingleton<ICacheProvider<ConnectedUser>, InMemoryCacheProvider<ConnectedUser>>(ConnectedUserService.ConnectedUsersCacheName);
builder.Services.AddKeyedSingleton<ICacheProvider<ConnectedUser>, InMemoryCacheProvider<ConnectedUser>>(SheetPresenceService.SheetPresenceCacheName);

builder.Services.AddSingleton<IConnectedUserService, ConnectedUserService>();
builder.Services.AddSingleton<ISheetPresenceService, SheetPresenceService>();

var app = builder.Build();

app.UseDocumentation(apiInfo);

app.UseResponseCompression();
app.UseHttpsRedirection();
app.UseRouting();
app.UseCors(CorsConstants.DefaultCorsPolicy);
app.UseResponseCaching();

app.MapControllers();
app.MapHub<CheckInOutHub>("/checkinouthub");

app.Run();
