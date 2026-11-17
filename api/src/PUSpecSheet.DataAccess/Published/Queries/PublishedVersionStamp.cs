namespace PUSpecSheet.DataAccess.Published.Queries;

/// <summary>A published version of a sheet: its number and when it was published.</summary>
public sealed record PublishedVersionStamp
{
    public required int SheetId { get; init; }

    public required int VersionNumber { get; init; }

    public required DateTime PublishedAtUtc { get; init; }
}
