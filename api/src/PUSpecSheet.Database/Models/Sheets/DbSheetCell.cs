using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// One cell of a sheet row, laid out from a <see cref="DbTemplateCell"/>. The cell itself never changes,
/// so its <see cref="PublicId"/> finds it in every version; its value in each row revision lives in the
/// typed tables of the "cell" schema.
/// </summary>
public class DbSheetCell
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the cell across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetRowId { get; set; }

    public DbSheetRow SheetRow { get; set; } = null!;

    public int TemplateCellId { get; set; }

    public DbTemplateCell TemplateCell { get; set; } = null!;

    /// <summary>
    /// The column block copy the cell belongs to, for a cell of a <see cref="DbTemplateColumnBlock"/>; null
    /// for the row's own cells. A row holds one cell per block template cell for every block copy.
    /// </summary>
    public int? SheetColumnBlockId { get; set; }

    public DbSheetColumnBlock? SheetColumnBlock { get; set; }
}
