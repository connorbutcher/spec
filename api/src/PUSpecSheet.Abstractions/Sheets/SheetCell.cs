using PUSpecSheet.Abstractions.CellTypes.InstanceSettings;

namespace PUSpecSheet.Abstractions.Sheets;

/// <summary>
/// A cell on a sheet: the template cell it was built from (<see cref="TemplateCellId"/>, found in the
/// sheet's <see cref="Sheet.TemplateCells"/>, which lists each one once) and its value.
/// </summary>
public sealed record SheetCell
{
    /// <summary>The cell's id.</summary>
    public required int Id { get; init; }

    /// <summary>The cell's id outside this database.</summary>
    public required Guid PublicId { get; init; }

    /// <summary>The template cell the cell was built from.</summary>
    public required int TemplateCellId { get; init; }

    /// <summary>
    /// The cell's value as the row shown holds it, in the shape its kind takes: a string for text and linked
    /// dropdown cells, a number for number cells, an ISO date (<c>yyyy-MM-dd</c>) for date cells, true or false
    /// for checkboxes, the chosen option's id for text and number dropdowns, and an object of slot key to text,
    /// in slot order, for labelled text cells. Null when the cell holds nothing.
    /// </summary>
    public object? Value { get; init; }

    /// <summary>The column block copy the cell belongs to, if any.</summary>
    public int? SheetColumnBlockId { get; init; }

    /// <summary>The last published change to the cell.</summary>
    public SheetChange? LastChange { get; init; }

    /// <summary>
    /// The settings chosen for this cell on the sheet, for a kind that has them (where a linked dropdown
    /// takes its choices from, for example); null when nothing has been chosen.
    /// </summary>
    public CellInstanceSettings? Settings { get; init; }
}
