using PUSpecSheet.Api.Emails.Models;

namespace PUSpecSheet.Api.Emails.Services;

public interface IEmailBuilderService
{
    EmailArgs BuildApprovalNotificationEmail(SpecSheetEmailArgs specSheetEmail);

    EmailArgs BuildPublishedNotificationEmail(SpecSheetEmailArgs specSheetEmail);

    EmailArgs BuildRejectionNotificationEmail(SpecSheetEmailArgs specSheetEmail);
}
