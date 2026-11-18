using Microsoft.Extensions.Configuration;
using PUSpecSheet.Api.Emails.Models;

namespace PUSpecSheet.Api.Emails.Services;

internal class EmailBuilderService : IEmailBuilderService
{
    private readonly string _appUrl;

    public EmailBuilderService(IConfiguration configuration)
    {
        _appUrl = configuration.GetValue<string>("PUSpecSheetApp:SiteUri")
            ?? throw new InvalidOperationException("App URL is not configured. Please set 'PUSpecSheetApp:SiteUri' in the configuration.");
    }

    public EmailArgs BuildApprovalNotificationEmail(SpecSheetEmailArgs specSheetEmail)
    {
        var notificationContent = new ApprovalNotificationContent
        {
            Name = specSheetEmail.User.DisplayName,
            Type = specSheetEmail.TypeName,
            Phase = specSheetEmail.PhaseName,
            ChangeSummary = specSheetEmail.Content,
            Url = BuildUrl(specSheetEmail.SpecSheetPhaseId, specSheetEmail.SpecSheetTypeId)
        };

        return new EmailArgs
        {
            To = [specSheetEmail.User.EmailAddress],
            Subject = $"PU Spec Sheet - {specSheetEmail.TypeName} - {specSheetEmail.PhaseName} - Approval needed",
            Important = true,
            BodyContent = notificationContent
        };
    }

    public EmailArgs BuildPublishedNotificationEmail(SpecSheetEmailArgs specSheetEmail)
    {
        var notificationContent = new PublishedNotificationContent
        {
            Name = specSheetEmail.User.DisplayName,
            Type = specSheetEmail.TypeName,
            Phase = specSheetEmail.PhaseName,
            ChangeSummary = specSheetEmail.Content,
            Url = BuildUrl(specSheetEmail.SpecSheetPhaseId, specSheetEmail.SpecSheetTypeId)
        };

        return new EmailArgs
        {
            To = [specSheetEmail.User.EmailAddress],
            Subject = $"PU Spec Sheet - {specSheetEmail.TypeName} - {specSheetEmail.PhaseName} - Approved",
            Important = true,
            BodyContent = notificationContent
        };
    }

    public EmailArgs BuildRejectionNotificationEmail(SpecSheetEmailArgs specSheetEmail)
    {
        var notificationContent = new RejectionNotificationContent
        {
            Name = specSheetEmail.User.DisplayName,
            Type = specSheetEmail.TypeName,
            Phase = specSheetEmail.PhaseName,
            RejectedBy = specSheetEmail.RejectedBy!,
            RejectionReason = specSheetEmail.Content,
            Url = BuildUrl(specSheetEmail.SpecSheetPhaseId, specSheetEmail.SpecSheetTypeId)
        };

        return new EmailArgs
        {
            To = [specSheetEmail.User.EmailAddress],
            Subject = $"PU Spec Sheet - {specSheetEmail.TypeName} - {specSheetEmail.PhaseName} - Rejected",
            Important = true,
            BodyContent = notificationContent
        };
    }

    private string BuildUrl(int specSheetPhaseId, int specSheetTypeId)
    {
        return $"{_appUrl}/phases/{specSheetPhaseId}/sheets/{specSheetTypeId}";
    }
}
