using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>The value of one template cell in one row revision, stored as text and parsed by its cell type.</summary>
public class SheetCellValue
{
    public int Id { get; set; }

    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int TemplateCellId { get; set; }

    public TemplateCell TemplateCell { get; set; } = null!;

    public string? Value { get; set; }
}
