using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// A row of a sheet section, laid out from a <see cref="TemplateRow"/>. The row is the unit that is
/// locked and versioned: its position and cell values live in <see cref="Revisions"/>.
/// </summary>
public class SheetRow
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the row across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetSectionId { get; set; }

    public SheetSection SheetSection { get; set; } = null!;

    public int TemplateRowId { get; set; }

    public TemplateRow TemplateRow { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SheetCell> Cells { get; set; } = [];

    public ICollection<SheetRowRevision> Revisions { get; set; } = [];
}
