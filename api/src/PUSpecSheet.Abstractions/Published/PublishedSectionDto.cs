using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Published;

/// <summary>A section of a published table: its own rows, then its sub-sections.</summary>
public sealed record PublishedSectionDto
{
    public required Guid Id { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    public required IReadOnlyList<PublishedRowDto> Rows { get; init; }

    public required IReadOnlyList<PublishedSectionDto> Sections { get; init; }
}
