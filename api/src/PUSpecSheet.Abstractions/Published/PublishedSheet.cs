using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Published;

/// <summary>
/// A sheet's published values at one version, for another application to read. Only one of
/// <see cref="Tables"/> (the tree shape) and <see cref="Cells"/> (the flat shape) is set. A version never
/// changes once published, so the same request for the same version always gives the same answer.
/// </summary>
public sealed record PublishedSheet
{
    public required Guid Sheet { get; init; }

    public required int Version { get; init; }

    public required DateTime PublishedAtUtc { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<PublishedTable>? Tables { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<Guid, object>? Cells { get; init; }

    public required IReadOnlyList<Guid> Missing { get; init; }
}
