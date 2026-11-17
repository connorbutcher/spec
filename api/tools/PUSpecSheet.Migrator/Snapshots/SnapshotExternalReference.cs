using System.Text.Json;

namespace PUSpecSheet.Migrator.Snapshots;

/// <summary>
/// A reference to a sheet snapshot from another system. <see cref="Signature"/> is everything but its ReferenceID, so
/// two are compared as text.
/// </summary>
internal sealed record SnapshotExternalReference
{
    public required Guid ReferenceId { get; init; }

    public required int ExternalId { get; init; }

    public required string SystemName { get; init; }

    public required string OwnerName { get; init; }

    public required string Url { get; init; }

    public required string Type { get; init; }

    public required string? Grouping { get; init; }

    public required string? SystemIconUrl { get; init; }

    public required string? Description { get; init; }

    public string Signature => JsonSerializer.Serialize(new object?[] { ExternalId, SystemName, OwnerName, Url, Type, Grouping, SystemIconUrl, Description });
}
