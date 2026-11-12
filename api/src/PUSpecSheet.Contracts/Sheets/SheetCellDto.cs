using PUSpecSheet.Domain.CellTypes.InstanceSettings;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A cell on a sheet: the template cell it was built from (<see cref="TemplateCellId"/>, found in the
/// sheet's <see cref="SheetDto.TemplateCells"/>, which lists each one once) and its value.
/// </summary>
/// <param name="Id">The cell's id.</param>
/// <param name="PublicId">The cell's id outside this database.</param>
/// <param name="TemplateCellId">The template cell the cell was built from.</param>
/// <param name="Value">
/// The cell's value as the row shown holds it, in the shape its kind takes: a string for text and linked
/// dropdown cells, a number for number cells, an ISO date (<c>yyyy-MM-dd</c>) for date cells, true or false
/// for checkboxes, the chosen option's id for text and number dropdowns, and an object of slot key to text,
/// in slot order, for labelled text cells. Null when the cell holds nothing.
/// </param>
/// <param name="SheetColumnBlockId">The column block copy the cell belongs to, if any.</param>
/// <param name="LastChange">The last published change to the cell.</param>
public sealed record SheetCellDto(
    int Id,
    Guid PublicId,
    int TemplateCellId,
    object? Value,
    int? SheetColumnBlockId,
    SheetChangeDto? LastChange)
{
    /// <summary>
    /// The settings chosen for this cell on the sheet, for a kind that has them (where a linked dropdown
    /// takes its choices from, for example); null when nothing has been chosen.
    /// </summary>
    public CellInstanceSettings? Settings { get; init; }
}
