using System.Text.Json;

namespace PUSpecSheet.Migrator.Snapshots;

internal sealed record SnapshotExternalReference
{
    public required Guid ReferenceId { get; init; }

    public int ExternalId { get; init; }

    public required string SystemName { get; init; }

    public required string OwnerName { get; init; }

    public required string Url { get; init; }

    public required string Type { get; init; }

    public string? Grouping { get; init; }

    public string? SystemIconUrl { get; init; }

    public string? Description { get; init; }

    public string Signature => JsonSerializer.Serialize(new object?[] { ExternalId, SystemName, OwnerName, Url, Type, Grouping, SystemIconUrl, Description });
}
