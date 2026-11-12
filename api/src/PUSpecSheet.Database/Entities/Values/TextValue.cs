using PUSpecSheet.Database.Entities.Sheets;

namespace PUSpecSheet.Database.Entities.Values;

/// <summary>The value of a text cell.</summary>
public class TextValue : ICellValue
{
    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public SheetCell SheetCell { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}
