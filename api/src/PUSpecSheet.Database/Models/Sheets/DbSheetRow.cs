using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// A row of a sheet section, laid out from a <see cref="DbTemplateRow"/>. The row is the unit that is
/// locked and versioned: its position and cell values live in <see cref="Revisions"/>.
/// </summary>
public class DbSheetRow
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the row across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetSectionId { get; set; }

    public int TemplateRowId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual DbSheetSection SheetSection { get; set; } = null!;

    public virtual DbTemplateRow TemplateRow { get; set; } = null!;

    public virtual ICollection<DbSheetCell> Cells { get; set; } = [];

    public virtual ICollection<DbSheetRowRevision> Revisions { get; set; } = [];
}
