using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>A new value for one cell of a row, with the kind of cell it is for, which decides where it is stored.</summary>
public sealed record CellValueChange(int SheetCellId, CellKind Kind, CellValueBag Value);
