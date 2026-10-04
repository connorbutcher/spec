using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>A section of a published table: its own rows, then its sub-sections.</summary>
public sealed record PublishedSectionDto(
    Guid Id,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Name,
    IReadOnlyList<PublishedRowDto> Rows,
    IReadOnlyList<PublishedSectionDto> Sections);
