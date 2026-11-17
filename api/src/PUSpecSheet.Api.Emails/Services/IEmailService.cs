using PUSpecSheet.Api.Emails.Models;

namespace PUSpecSheet.Api.Emails.Services;

public interface IEmailService
{
    Task SendApprovalEmailAsync(EmailArgs emailArgs, CancellationToken cancellationToken);

    Task SendRejectionEmailAsync(EmailArgs emailArgs, CancellationToken cancellationToken);

    Task SendPublishedEmailAsync(EmailArgs emailArgs, CancellationToken cancellationToken);
}
