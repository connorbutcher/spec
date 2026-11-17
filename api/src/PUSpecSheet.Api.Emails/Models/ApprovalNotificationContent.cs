namespace PUSpecSheet.Api.Emails.Models;

public record ApprovalNotificationContent
{
    public required string Name { get; init; }

    public required string Type { get; init; }

    public required string Phase { get; init; }

    public required string ChangeSummary { get; init; }

    public required string Url { get; init; }
}
