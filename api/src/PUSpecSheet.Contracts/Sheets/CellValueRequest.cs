using System.Text.Json;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>A cell's new value.</summary>
/// <param name="SheetCellId">The cell to set.</param>
/// <param name="Value">
/// The value in the shape the cell's kind takes, the same as <see cref="SheetCellDto.Value"/>: a string for
/// text and linked dropdown cells, a number for number cells, an ISO date (<c>yyyy-MM-dd</c>) for date cells,
/// true or false for checkboxes, an option id for text and number dropdowns, and an object of slot key to
/// text for labelled text cells. Null, or leaving it out, clears the cell.
/// </param>
public sealed record CellValueRequest(int SheetCellId, JsonElement? Value);
