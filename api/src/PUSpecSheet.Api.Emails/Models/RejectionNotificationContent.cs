namespace PUSpecSheet.Api.Emails.Models;

public record RejectionNotificationContent
{
    public required string Name { get; init; }

    public required string RejectedBy { get; init; }

    public required string Type { get; init; }

    public required string Phase { get; init; }

    public required string RejectionReason { get; init; }

    public required string Url { get; init; }
}
