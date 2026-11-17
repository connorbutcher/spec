namespace PUSpecSheet.Abstractions.Published;

/// <summary>
/// Rows of a published sheet at one version, by row identifier, so a caller that stores row identifiers
/// reads each one straight out of <see cref="Rows"/>. <see cref="Missing"/> lists the identifiers asked
/// for that the version doesn't hold.
/// </summary>
public sealed record PublishedRows
{
    public required Guid Sheet { get; init; }

    public required int Version { get; init; }

    public required DateTime PublishedAtUtc { get; init; }

    public required IReadOnlyDictionary<Guid, PublishedRowValues> Rows { get; init; }

    public required IReadOnlyList<Guid> Missing { get; init; }
}
