using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>The value of a number cell.</summary>
public class DbNumericValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public int SheetCellId { get; set; }

    public decimal Value { get; set; }

    public virtual DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public virtual DbSheetCell SheetCell { get; set; } = null!;
}
