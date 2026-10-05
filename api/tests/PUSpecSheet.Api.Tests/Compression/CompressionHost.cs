using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PUSpecSheet.Api.Compression;

namespace PUSpecSheet.Api.Tests.Compression;

/// <summary>A small API host with the real compression setup and the test controller, served in memory.</summary>
internal static class CompressionHost
{
    public static async Task<(WebApplication App, HttpClient Client)> StartAsync(IDictionary<string, string?>? settings = null)
    {
        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseTestServer();
        builder.Configuration.AddInMemoryCollection(settings ?? new Dictionary<string, string?>());
        builder.Services.AddControllers().AddApplicationPart(typeof(CompressionTestController).Assembly);
        builder.Services.AddProblemDetails();
        builder.Services.AddPuSpecSheetResponseCompression(builder.Configuration);

        var app = builder.Build();
        app.UsePuSpecSheetResponseCompression();
        app.MapControllers();
        await app.StartAsync();

        // The client must not decompress for us: the tests look at what is sent.
        var client = app.GetTestServer().CreateClient();
        return (app, client);
    }
}
