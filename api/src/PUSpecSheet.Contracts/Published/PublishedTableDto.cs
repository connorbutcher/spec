using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// A table on a published sheet. <see cref="Title"/> is only sent when labels are asked for.
/// <see cref="Columns"/> lists the table's sets of repeated columns, left to right, when it has any.
/// </summary>
public sealed record PublishedTableDto(
    Guid Id,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Title,
    IReadOnlyList<PublishedSectionDto> Sections,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<PublishedColumnBlockDto>? Columns);
