namespace PUSpecSheet.Application.Published.Queries;

/// <summary>A published version of a sheet: its number and when it was published.</summary>
public sealed record PublishedVersionStamp
{
    public PublishedVersionStamp(int sheetId, int versionNumber, DateTime publishedAtUtc)
    {
        this.SheetId = sheetId;
        this.VersionNumber = versionNumber;
        this.PublishedAtUtc = publishedAtUtc;
    }

    public int SheetId { get; init; }

    public int VersionNumber { get; init; }

    public DateTime PublishedAtUtc { get; init; }
}
