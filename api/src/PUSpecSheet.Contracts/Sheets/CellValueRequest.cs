using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>
/// A cell's new value. Set the field that matches the cell's kind: text for text cells, number for
/// number cells, date for date cells, boolean for checkboxes and option for dropdowns (text or number).
/// Leaving it unset clears the cell.
/// </summary>
public sealed record CellValueRequest(
    int SheetCellId,
    [StringLength(4000)] string? Text,
    decimal? Number,
    DateOnly? Date,
    bool? Boolean,
    int? OptionId);
