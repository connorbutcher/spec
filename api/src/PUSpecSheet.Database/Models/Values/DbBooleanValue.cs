using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>The value of a checkbox cell.</summary>
public class DbBooleanValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public int SheetCellId { get; set; }

    public bool Value { get; set; }

    public virtual DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public virtual DbSheetCell SheetCell { get; set; } = null!;
}
