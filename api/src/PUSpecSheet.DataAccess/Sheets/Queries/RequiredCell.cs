using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Sheets.Queries;

/// <summary>A sheet cell whose template cell must be filled in: which cell, its row and its kind.</summary>
public sealed record RequiredCell
{
    public required int Id { get; init; }

    public required int SheetRowId { get; init; }

    public required CellKind Kind { get; init; }
}
