namespace PUSpecSheet.Api.Emails.Models;

public record Email
{
    public required string[] To { get; init; }

    public required string From { get; init; }

    public string[]? Cc { get; init; }

    public string[]? Bcc { get; init; }

    public required string Subject { get; init; }

    public required string Content { get; init; }

    public required bool IsHighPriority { get; init; }

    public required string TemplateName { get; init; }

    public required string TemplateAccessCode { get; init; }
}
