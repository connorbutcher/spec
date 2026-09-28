using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Values;

/// <summary>The value of a checkbox cell.</summary>
public class BooleanValue : ICellValue
{
    public int Id { get; set; }

    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int TemplateCellId { get; set; }

    public TemplateCell TemplateCell { get; set; } = null!;

    public bool Value { get; set; }
}
