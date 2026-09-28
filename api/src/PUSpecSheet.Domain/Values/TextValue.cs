using PUSpecSheet.Domain.Sheets;

namespace PUSpecSheet.Domain.Values;

/// <summary>The value of a text cell.</summary>
public class TextValue : ICellValue
{
    public int Id { get; set; }

    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int SheetCellId { get; set; }

    public SheetCell SheetCell { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}
