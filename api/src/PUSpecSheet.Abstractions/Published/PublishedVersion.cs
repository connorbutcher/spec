namespace PUSpecSheet.Abstractions.Published;

/// <summary>One published version of a sheet.</summary>
public sealed record PublishedVersion
{
    public required int Version { get; init; }

    public required DateTime PublishedAtUtc { get; init; }

    public string? Note { get; init; }
}
