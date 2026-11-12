using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>A section of a published table: its own rows, then its sub-sections.</summary>
public sealed record PublishedSectionDto
{
    public PublishedSectionDto(Guid id, string? name, IReadOnlyList<PublishedRowDto> rows, IReadOnlyList<PublishedSectionDto> sections)
    {
        this.Id = id;
        this.Name = name;
        this.Rows = rows;
        this.Sections = sections;
    }

    public Guid Id { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Name { get; init; }

    public IReadOnlyList<PublishedRowDto> Rows { get; init; }

    public IReadOnlyList<PublishedSectionDto> Sections { get; init; }
}
