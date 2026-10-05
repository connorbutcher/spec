using System.Text.Json.Serialization;

namespace PUSpecSheet.Contracts.Published;

/// <summary>
/// One place a looked-up value was found, at one version of its sheet, with what belongs to it.
///
/// When the cell is in a column block (a part number over a part's columns), <see cref="Block"/> names
/// the block, <see cref="Rows"/> are the table's rows with each row's description and the block's values,
/// and <see cref="Columns"/> are the block's headings.
///
/// Otherwise the cell is one of a row's own cells. <see cref="Rows"/> are then the rows of the cell's
/// section and its sub-sections (or of the whole table when the cell is in the header), each with its own
/// values, <see cref="Columns"/> are the table's own headings, and <see cref="Blocks"/> lists the table's
/// column blocks, if it has any.
/// </summary>
public sealed record PublishedLookupMatchDto(
    Guid Sheet,
    string Phase,
    int SheetType,
    int Version,
    DateTime PublishedAtUtc,
    Guid Table,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] string? Title,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] Guid? Block,
    IReadOnlyList<string> Columns,
    [property: JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)] IReadOnlyList<PublishedLookupBlockDto>? Blocks,
    IReadOnlyList<PublishedLookupRowDto> Rows);
