using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// One row of a published sheet at one version. <see cref="Section"/>, <see cref="Values"/> and
/// <see cref="Columns"/> are as in <see cref="PublishedRowValuesDto"/>.
/// </summary>
public sealed record PublishedSheetRowDto(
    Guid Sheet,
    int Version,
    DateTime PublishedAtUtc,
    Guid Row,
    string Section,
    IReadOnlyList<object?> Values,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<string, IReadOnlyList<object?>>? Columns)
{
    /// <summary>
    /// For a linked dropdown that also gives its choice's row: the rows its picked values came from, by the
    /// value picked. Read one with the rows endpoint of the same sheet and version to get that row's values.
    /// Left out when the row has no such cell, or the value is no longer in the column it was picked from.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, Guid>? LinkedRows { get; init; }
}
