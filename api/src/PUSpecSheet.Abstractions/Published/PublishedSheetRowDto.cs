using System.Text.Json.Serialization;

namespace PUSpecSheet.Abstractions.Published;

/// <summary>
/// One row of a published sheet at one version. <see cref="Section"/>, <see cref="Values"/> and
/// <see cref="Columns"/> are as in <see cref="PublishedRowValuesDto"/>.
/// </summary>
public sealed record PublishedSheetRowDto
{
    public required Guid Sheet { get; init; }

    public required int Version { get; init; }

    public required DateTime PublishedAtUtc { get; init; }

    public required Guid Row { get; init; }

    public required string Section { get; init; }

    public required IReadOnlyList<object?> Values { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, IReadOnlyList<object?>>? Columns { get; init; }
}
