namespace PUSpecSheet.Api.Emails.Models;

internal record NotificationsServiceSettings
{
    public required string BaseUrl { get; init; }

    public required string ApprovalTemplateName { get; init; }

    public required string ApprovalTemplateAccessCode { get; init; }

    public required string RejectionTemplateName { get; init; }

    public required string RejectionTemplateAccessCode { get; init; }

    public required string PublishedTemplateName { get; init; }

    public required string PublishedTemplateAccessCode { get; init; }

    public required string FromAddress { get; init; }
}
