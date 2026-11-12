using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Database.Entities.Values;

/// <summary>The value of a date cell.</summary>
public class DateValue : ICellValue
{
    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public SheetCell SheetCell { get; set; } = null!;

    public DateOnly Value { get; set; }
}
