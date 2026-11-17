namespace PUSpecSheet.DataAccess.Published.Queries;

public sealed record PublishedVersionStamp
{
    public required int SheetId { get; init; }

    public required int VersionNumber { get; init; }

    public required DateTime PublishedAtUtc { get; init; }
}
