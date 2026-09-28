using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// One cell of a sheet row, laid out from a <see cref="TemplateCell"/>. The cell itself never changes,
/// so its <see cref="PublicId"/> finds it in every version; its value in each row revision lives in the
/// typed tables of the "values" schema.
/// </summary>
public class SheetCell
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the cell across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetRowId { get; set; }

    public SheetRow SheetRow { get; set; } = null!;

    public int TemplateCellId { get; set; }

    public TemplateCell TemplateCell { get; set; } = null!;
}
