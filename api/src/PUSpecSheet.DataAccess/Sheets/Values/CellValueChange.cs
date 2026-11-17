using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Sheets.Values;

public sealed record CellValueChange
{
    public required int SheetCellId { get; init; }

    public required CellKind Kind { get; init; }

    public required CellValueBag Value { get; init; }
}
