using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace PUSpecSheet.Api.Common.Logging;

public static class SerilogApplicationBuilderExtensions
{
    public static WebApplicationBuilder AddSerilog(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton<IdentityLogEnrichmentFilter>();
        builder.Services.AddSingleton<HostLogEnrichmentFilter>();

        builder.Host.UseSerilog((host, serviceProvider, loggerConfiguration) =>
        {
            var identityEnricher = serviceProvider.GetRequiredService<IdentityLogEnrichmentFilter>();
            var hostEnricher = serviceProvider.GetRequiredService<HostLogEnrichmentFilter>();

            loggerConfiguration
                .ReadFrom.Configuration(host.Configuration)
                .Enrich.FromLogContext()
                .Enrich.With(identityEnricher)
                .Enrich.With(hostEnricher);
        });

        return builder;
    }
}
