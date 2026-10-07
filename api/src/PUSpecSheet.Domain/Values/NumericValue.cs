using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Domain.Values;

/// <summary>The value of a number cell.</summary>
public class NumericValue : ICellValue
{
    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public SheetCell SheetCell { get; set; } = null!;

    public decimal Value { get; set; }
}
