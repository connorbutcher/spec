using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Domain.Values;

/// <summary>The value of a date cell.</summary>
public class DateValue : ICellValue
{
    public int Id { get; set; }

    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public SheetCell SheetCell { get; set; } = null!;

    public DateOnly Value { get; set; }
}
