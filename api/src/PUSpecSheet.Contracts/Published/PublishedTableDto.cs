using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// A table on a published sheet. <see cref="Title"/> is only sent when labels are asked for.
/// <see cref="Columns"/> lists the table's sets of repeated columns, left to right, when it has any.
/// </summary>
public sealed record PublishedTableDto
{
    public PublishedTableDto(
        Guid id,
        string? title,
        IReadOnlyList<PublishedSectionDto> sections,
        IReadOnlyList<PublishedColumnBlockDto>? columns)
    {
        this.Id = id;
        this.Title = title;
        this.Sections = sections;
        this.Columns = columns;
    }

    public Guid Id { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    public IReadOnlyList<PublishedSectionDto> Sections { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<PublishedColumnBlockDto>? Columns { get; init; }
}
