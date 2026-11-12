using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>A new value for one cell of a row, with the kind of cell it is for, which decides where it is stored.</summary>
public sealed record CellValueChange
{
    public CellValueChange(int sheetCellId, CellKind kind, CellValueBag value)
    {
        this.SheetCellId = sheetCellId;
        this.Kind = kind;
        this.Value = value;
    }

    public int SheetCellId { get; init; }

    public CellKind Kind { get; init; }

    public CellValueBag Value { get; init; }
}
