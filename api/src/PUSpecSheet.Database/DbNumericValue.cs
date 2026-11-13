namespace PUSpecSheet.Database;

/// <summary>The value of a number cell.</summary>
public class DbNumericValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public DbSheetCell SheetCell { get; set; } = null!;

    public decimal Value { get; set; }
}
