namespace PUSpecSheet.Database;

/// <summary>The value of a date cell.</summary>
public class DbDateValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public DbSheetCell SheetCell { get; set; } = null!;

    public DateOnly Value { get; set; }
}
