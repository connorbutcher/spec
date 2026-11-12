using PUSpecSheet.Database.Entities.CellTypes;
using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Database.Entities.Values;

/// <summary>The value of a dropdown cell: one of its cell type's options.</summary>
public class OptionValue : ICellValue
{
    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public SheetCell SheetCell { get; set; } = null!;

    public int CellTypeOptionId { get; set; }

    public CellTypeOption CellTypeOption { get; set; } = null!;
}
