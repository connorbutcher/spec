using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>A cell of a newly added row, with the kind of its cell type.</summary>
public sealed record NewCell
{
    public NewCell(int id, int sheetRowId, int templateCellId, int? sheetColumnBlockId, CellKind kind)
    {
        this.Id = id;
        this.SheetRowId = sheetRowId;
        this.TemplateCellId = templateCellId;
        this.SheetColumnBlockId = sheetColumnBlockId;
        this.Kind = kind;
    }

    public int Id { get; init; }

    public int SheetRowId { get; init; }

    public int TemplateCellId { get; init; }

    public int? SheetColumnBlockId { get; init; }

    public CellKind Kind { get; init; }
}
