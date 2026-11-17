using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Published;

/// <summary>
/// A table on a published sheet. <see cref="Title"/> is only sent when labels are asked for.
/// <see cref="Columns"/> lists the table's sets of repeated columns, left to right, when it has any.
/// </summary>
public sealed record PublishedTable
{
    public required Guid Id { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Title { get; init; }

    public required IReadOnlyList<PublishedSection> Sections { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyList<PublishedColumnBlock>? Columns { get; init; }
}
