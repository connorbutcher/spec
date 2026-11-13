using PUSpecSheet.Database.Models.Sheets;

namespace PUSpecSheet.Database.Models.Values;

/// <summary>The value of a text cell.</summary>
public class DbTextValue : IDbCellValue
{
    public int SheetRowRevisionId { get; set; }

    public int SheetCellId { get; set; }

    public string Value { get; set; } = string.Empty;

    public virtual DbSheetRowRevision SheetRowRevision { get; set; } = null!;

    public virtual DbSheetCell SheetCell { get; set; } = null!;
}
