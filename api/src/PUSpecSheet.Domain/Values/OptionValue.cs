using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Domain.Values;

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
