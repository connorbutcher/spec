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
public sealed record PublishedRowValuesDto
{
    public PublishedRowValuesDto(
        string section,
        IReadOnlyList<object?> values,
        IReadOnlyDictionary<string, IReadOnlyList<object?>>? columns)
    {
        this.Section = section;
        this.Values = values;
        this.Columns = columns;
    }

    public string Section { get; init; }

    public IReadOnlyList<object?> Values { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IReadOnlyDictionary<string, IReadOnlyList<object?>>? Columns { get; init; }
}
