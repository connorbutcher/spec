using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Published;

/// <summary>A section of a published table: its own rows, then its sub-sections.</summary>
public sealed record PublishedSectionDto
{
    public PublishedSectionDto(Guid id, string? name, IReadOnlyList<PublishedRowDto> rows, IReadOnlyList<PublishedSectionDto> sections)
    {
        Id = id;
        Name = name;
        Rows = rows;
        Sections = sections;
    }

    public Guid Id { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    public IReadOnlyList<PublishedRowDto> Rows { get; init; }

    public IReadOnlyList<PublishedSectionDto> Sections { get; init; }
}
