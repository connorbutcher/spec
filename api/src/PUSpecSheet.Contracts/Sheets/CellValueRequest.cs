using System.Text.Json;

namespace PUSpecSheet.Contracts.Sheets;

/// <summary>A cell's new value.</summary>
public sealed record CellValueRequest
{
    public CellValueRequest(int sheetCellId, JsonElement? value)
    {
        this.SheetCellId = sheetCellId;
        this.Value = value;
    }

    /// <summary>The cell to set.</summary>
    public int SheetCellId { get; init; }

    /// <summary>
    /// The value in the shape the cell's kind takes, the same as <see cref="SheetCellDto.Value"/>: a string for
    /// text and linked dropdown cells, a number for number cells, an ISO date (<c>yyyy-MM-dd</c>) for date cells,
    /// true or false for checkboxes, an option id for text and number dropdowns, and an object of slot key to
    /// text for labelled text cells. Null, or leaving it out, clears the cell.
    /// </summary>
    public JsonElement? Value { get; init; }
}
