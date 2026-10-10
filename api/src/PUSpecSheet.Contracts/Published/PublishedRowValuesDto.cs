using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// One row's data. <see cref="Section"/> is the kind of row, named in the template, such as "Limits".
/// <see cref="Values"/> are the row's own cells, left to right, with null for an empty cell.
///
/// A row of a table with repeated columns, such as one set of columns per part, also has
/// <see cref="Columns"/>: the row's values in each set, by the set's name. The name is the value at the
/// top of the set, such as the part number "P-1003", or the set's identifier when that is empty or used
/// twice in the table.
/// </summary>
public sealed record PublishedRowValuesDto(
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
