using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Domain.Values;

/// <summary>The value of a checkbox cell.</summary>
public class BooleanValue : ICellValue
{
    public int Id { get; set; }

    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public SheetCell SheetCell { get; set; } = null!;

    public bool Value { get; set; }
}
