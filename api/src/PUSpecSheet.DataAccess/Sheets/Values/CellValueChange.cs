using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Sheets.Values;

/// <summary>A new value for one cell of a row, with the kind of cell it is for, which decides where it is stored.</summary>
public sealed record CellValueChange
{
    public required int SheetCellId { get; init; }

    public required CellKind Kind { get; init; }

    public required CellValueBag Value { get; init; }
}
