using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// A row that belongs to a looked-up value. <see cref="Section"/> is the name of the kind of row, from the
/// template. <see cref="Values"/> has one entry per cell, left to right, null for an empty cell; a row
/// with fewer entries than there are columns has cells that span several columns.
///
/// For a match in a column block, <see cref="Values"/> are the block's cells in the row and
/// <see cref="Description"/> is the row's own cell beside them (a list when the row has several, left out
/// when they are all empty). Otherwise <see cref="Values"/> are the row's own cells and
/// <see cref="Blocks"/> holds the row's values in each column block of the table.
/// </summary>
public sealed record PublishedLookupRowDto(
    Guid Row,
    string Section,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] object? Description,
    IReadOnlyList<object?> Values,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<PublishedLookupBlockValuesDto>? Blocks);
