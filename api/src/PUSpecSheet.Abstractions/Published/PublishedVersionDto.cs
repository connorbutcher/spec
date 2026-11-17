namespace PUSpecSheet.Abstractions.Published;

/// <summary>One published version of a sheet.</summary>
public sealed record PublishedVersionDto
{
    public PublishedVersionDto(int version, DateTime publishedAtUtc, string? note)
    {
        Version = version;
        PublishedAtUtc = publishedAtUtc;
        Note = note;
    }

    public int Version { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    public string? Note { get; init; }
}
