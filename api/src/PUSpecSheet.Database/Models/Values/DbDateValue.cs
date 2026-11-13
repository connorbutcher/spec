using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>The value of a date cell.</summary>
public class DbDateValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public int SheetCellId { get; set; }

    public DateOnly Value { get; set; }

    public virtual DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public virtual DbSheetCell SheetCell { get; set; } = null!;
}
