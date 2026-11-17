using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Sheets.Values;

/// <summary>A cell of a newly added row, with the kind of its cell type.</summary>
public sealed record NewCell
{
    public NewCell(int id, int sheetRowId, int templateCellId, int? sheetColumnBlockId, CellKind kind)
    {
        Id = id;
        SheetRowId = sheetRowId;
        TemplateCellId = templateCellId;
        SheetColumnBlockId = sheetColumnBlockId;
        Kind = kind;
    }

    public int Id { get; init; }

    public int SheetRowId { get; init; }

    public int TemplateCellId { get; init; }

    public int? SheetColumnBlockId { get; init; }

    public CellKind Kind { get; init; }
}
