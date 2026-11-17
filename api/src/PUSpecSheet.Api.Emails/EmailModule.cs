using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using PUSpecSheet.Api.Emails.Models;
using PUSpecSheet.Api.Emails.Services;

namespace PUSpecSheet.Api.Emails;

/// <summary>
/// The email module.
/// </summary>
public static class EmailModule
{
    /// <summary>
    /// Adds data access layer declarations to the builder.
    /// </summary>
    /// <param name="builder">The api builder.</param>
    public static void AddEmailModule(this IHostApplicationBuilder builder)
    {
        builder.Services.Configure<NotificationsServiceSettings>(builder.Configuration.GetSection("Services:Notifications"));
        builder.Services.AddHttpClient("NotificationService", (serviceProvider, httpClient) =>
        {
            var configuration = serviceProvider.GetRequiredService<IOptions<NotificationsServiceSettings>>().Value;
            httpClient.BaseAddress = new Uri(configuration.BaseUrl);
        });

        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailBuilderService, EmailBuilderService>();
    }
}