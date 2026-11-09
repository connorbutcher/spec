using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A cell's new value. Set the field that matches the cell's kind: text for text cells, number for
/// number cells, date for date cells, boolean for checkboxes, option for dropdowns (text or number) and
/// slots for labelled text cells. Leaving it unset clears the cell.
/// </summary>
public sealed record CellValueRequest(
    int SheetCellId,
    [StringLength(4000)] string? Text,
    decimal? Number,
    DateOnly? Date,
    bool? Boolean,
    int? OptionId)
{
    /// <summary>A labelled text cell's values by slot key, in the cell's order, such as { "rh": "P-1003-R" }. Every value the cell keeps is sent: a slot left out is cleared.</summary>
    [MaxLength(8)]
    public IReadOnlyDictionary<string, string>? Slots { get; init; }
}
