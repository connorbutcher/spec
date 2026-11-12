namespace PUSpecSheet.Contracts.Published;

/// <summary>One published version of a sheet.</summary>
public sealed record PublishedVersionDto
{
    public PublishedVersionDto(int version, DateTime publishedAtUtc, string? note)
    {
        this.Version = version;
        this.PublishedAtUtc = publishedAtUtc;
        this.Note = note;
    }

    public int Version { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    public string? Note { get; init; }
}
