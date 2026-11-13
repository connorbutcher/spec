namespace PUSpecSheet.Database;

/// <summary>The value of a checkbox cell.</summary>
public class DbBooleanValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public DbSheetCell SheetCell { get; set; } = null!;

    public bool Value { get; set; }
}
