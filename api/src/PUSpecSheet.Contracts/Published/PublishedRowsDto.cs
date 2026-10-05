namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// Rows of a published sheet at one version, by row identifier, so a caller that stores row identifiers
/// reads each one straight out of <see cref="Rows"/>. <see cref="Missing"/> lists the identifiers asked
/// for that the version doesn't hold.
/// </summary>
public sealed record PublishedRowsDto(
    Guid Sheet,
    int Version,
    DateTime PublishedAtUtc,
    IReadOnlyDictionary<Guid, PublishedRowValuesDto> Rows,
    IReadOnlyList<Guid> Missing);
