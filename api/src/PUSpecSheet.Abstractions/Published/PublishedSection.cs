using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Published;

/// <summary>A section of a published table: its own rows, then its sub-sections.</summary>
public sealed record PublishedSection
{
    public required Guid Id { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    public required IReadOnlyList<PublishedRow> Rows { get; init; }

    public required IReadOnlyList<PublishedSection> Sections { get; init; }
}
