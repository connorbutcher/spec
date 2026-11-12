using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Persistence.Sheets;

/// <summary>A sheet cell whose template cell must be filled in: which cell, its row and its kind.</summary>
public sealed record RequiredCell
{
    public RequiredCell(int id, int sheetRowId, CellKind kind)
    {
        this.Id = id;
        this.SheetRowId = sheetRowId;
        this.Kind = kind;
    }

    public int Id { get; init; }

    public int SheetRowId { get; init; }

    public CellKind Kind { get; init; }
}
