namespace PUSpecSheet.Api.Emails.Models;

public record EmailArgs
{
    public string[] To { get; init; } = [];

    public string[] Cc { get; init; } = [];

    public string[] Bcc { get; init; } = [];

    public required string Subject { get; init; }

    public bool Important { get; init; } = true;

    public required dynamic BodyContent { get; init; }
}
