namespace PUSpecSheet.Database;

/// <summary>The value of a dropdown cell: one of its cell type's options.</summary>
public class DbOptionValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public DbSheetCell SheetCell { get; set; } = null!;

    public int CellTypeOptionId { get; set; }

    public DbCellTypeOption CellTypeOption { get; set; } = null!;
}
