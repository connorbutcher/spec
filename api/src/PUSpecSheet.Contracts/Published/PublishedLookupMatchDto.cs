using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// One place a looked-up value was found, at one version of its sheet, with the rows that belong to it by
/// row identifier, in the same shape as reading rows by identifier.
///
/// When the value is at the top of a set of repeated columns (a part number over a part's columns),
/// <see cref="Column"/> is that set's name, <see cref="Rows"/> are the table's rows, each with the set's
/// values under that name in its <c>columns</c>, and <see cref="Headings"/> are the set's headings.
///
/// Otherwise the value is in one of a row's own cells. <see cref="Rows"/> are then the rows of the cell's
/// section and its sub-sections (or of the whole table when the cell is in the header), and
/// <see cref="Headings"/> are the table's own headings.
/// </summary>
public sealed record PublishedLookupMatchDto(
    Guid Sheet,
    string Phase,
    int SheetType,
    int Version,
    DateTime PublishedAtUtc,
    Guid Table,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Title,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Column,
    IReadOnlyList<string> Headings,
    IReadOnlyDictionary<Guid, PublishedRowValuesDto> Rows);
