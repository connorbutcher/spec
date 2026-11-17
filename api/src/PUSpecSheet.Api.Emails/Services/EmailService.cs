using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using PUSpecSheet.Api.Emails.Models;

namespace PUSpecSheet.Api.Emails.Services;

internal class EmailService : IEmailService
{
    private readonly HttpClient _notificationsServiceClient;
    private readonly NotificationsServiceSettings _notificationServiceSettings;
    private readonly ILogger<IEmailService> _logger;

    public EmailService(IHttpClientFactory httpClientFactory,
        IOptions<NotificationsServiceSettings> notificationServiceSettings,
        ILogger<IEmailService> logger)
    {
        _notificationsServiceClient = httpClientFactory.CreateClient("NotificationService");
        _notificationServiceSettings = notificationServiceSettings.Value;
        _logger = logger;
    }

    public async Task SendApprovalEmailAsync(EmailArgs emailArgs, CancellationToken cancellationToken)
    {
        var email = BuildEmail(emailArgs, _notificationServiceSettings.ApprovalTemplateName, _notificationServiceSettings.ApprovalTemplateAccessCode);
        await SendEmailAsync(email, cancellationToken);
    }

    public async Task SendRejectionEmailAsync(EmailArgs emailArgs, CancellationToken cancellationToken)
    {
        var email = BuildEmail(emailArgs, _notificationServiceSettings.RejectionTemplateName, _notificationServiceSettings.RejectionTemplateAccessCode);
        await SendEmailAsync(email, cancellationToken);
    }

    public async Task SendPublishedEmailAsync(EmailArgs emailArgs, CancellationToken cancellationToken)
    {
        var email = BuildEmail(emailArgs, _notificationServiceSettings.PublishedTemplateName, _notificationServiceSettings.PublishedTemplateAccessCode);
        await SendEmailAsync(email, cancellationToken);
    }

    private async Task SendEmailAsync(Email email, CancellationToken cancellationToken)
    {
        var response = await _notificationsServiceClient.PostAsJsonAsync("notifications/asap/email", email, cancellationToken);
        response.EnsureSuccessStatusCode();

        _logger.LogInformation("Email sent successfully to {Recipients} with subject '{Subject}'", string.Join(", ", email.To), email.Subject);
    }

    private Email BuildEmail(EmailArgs emailArgs, string templateName, string templateAccessCode)
    {
        return new Email
        {
            To = emailArgs.To,
            From = _notificationServiceSettings.FromAddress,
            Cc = emailArgs.Cc,
            Bcc = emailArgs.Bcc,
            Subject = emailArgs.Subject,
            Content = JsonSerializer.Serialize(emailArgs.BodyContent),
            IsHighPriority = emailArgs.Important,
            TemplateName = templateName,
            TemplateAccessCode = templateAccessCode
        };
    }
}
