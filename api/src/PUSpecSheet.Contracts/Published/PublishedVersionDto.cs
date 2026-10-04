namespace PUSpecSheet.Contracts.Published;

/// <summary>One published version of a sheet.</summary>
public sealed record PublishedVersionDto(
    int Version,
    DateTime PublishedAtUtc,
    string? Note);
