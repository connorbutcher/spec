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

    public DbSheetSection SheetSection { get; set; } = null!;

    public int TemplateRowId { get; set; }

    public DbTemplateRow TemplateRow { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<DbSheetCell> Cells { get; set; } = [];

    public ICollection<DbSheetRowRevision> Revisions { get; set; } = [];
}
