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
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyDictionary<string, IReadOnlyList<object?>>? Columns);
