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
            // A placeholder BaseUrl (as in local development) leaves the client without a base address,
            // so sending an email fails rather than every request that resolves the email service.
            if (Uri.TryCreate(configuration.BaseUrl, UriKind.Absolute, out var baseAddress))
            {
                httpClient.BaseAddress = baseAddress;
            }
        });

        builder.Services.AddScoped<IEmailService, EmailService>();
        builder.Services.AddScoped<IEmailBuilderService, EmailBuilderService>();
    }
}