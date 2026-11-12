namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// Rows of a published sheet at one version, by row identifier, so a caller that stores row identifiers
/// reads each one straight out of <see cref="Rows"/>. <see cref="Missing"/> lists the identifiers asked
/// for that the version doesn't hold.
/// </summary>
public sealed record PublishedRowsDto
{
    public PublishedRowsDto(
        Guid sheet,
        int version,
        DateTime publishedAtUtc,
        IReadOnlyDictionary<Guid, PublishedRowValuesDto> rows,
        IReadOnlyList<Guid> missing)
    {
        this.Sheet = sheet;
        this.Version = version;
        this.PublishedAtUtc = publishedAtUtc;
        this.Rows = rows;
        this.Missing = missing;
    }

    public Guid Sheet { get; init; }

    public int Version { get; init; }

    public DateTime PublishedAtUtc { get; init; }

    public IReadOnlyDictionary<Guid, PublishedRowValuesDto> Rows { get; init; }

    public IReadOnlyList<Guid> Missing { get; init; }
}
