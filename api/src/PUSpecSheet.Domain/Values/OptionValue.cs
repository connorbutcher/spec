using PUSpecSheet.Domain.CellTypes;
using PUSpecSheet.Domain.Sheets;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Values;

/// <summary>The value of a dropdown cell: one of its cell type's options.</summary>
public class OptionValue : ICellValue
{
    public int Id { get; set; }

    public int SheetRowRevisionId { get; set; }

    public SheetRowRevision SheetRowRevision { get; set; } = null!;

    public int TemplateCellId { get; set; }

    public TemplateCell TemplateCell { get; set; } = null!;

    public int CellTypeOptionId { get; set; }

    public CellTypeOption CellTypeOption { get; set; } = null!;
}
