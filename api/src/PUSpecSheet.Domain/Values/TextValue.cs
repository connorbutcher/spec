using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Values;

/// <summary>The value of a text cell.</summary>
public class TextValue : ICellValue
{
    public int Id { get; set; }

    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int TemplateCellId { get; set; }

    public TemplateCell TemplateCell { get; set; } = null!;

    public string Value { get; set; } = string.Empty;
}
