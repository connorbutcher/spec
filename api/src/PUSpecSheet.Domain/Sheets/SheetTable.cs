using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// A table on a sheet, laid out from a <see cref="TableTemplateVersion"/> of the sheet's type. Its title,
/// position and whether it's on the sheet are versioned in <see cref="Revisions"/>.
/// </summary>
public class SheetTable
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the table across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetId { get; set; }

    public Sheet Sheet { get; set; } = null!;

    /// <summary>The template version the table was built from. The table keeps this layout.</summary>
    public int TableTemplateVersionId { get; set; }

    public TableTemplateVersion TableTemplateVersion { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SheetTableRevision> Revisions { get; set; } = [];

    public ICollection<SheetSection> Sections { get; set; } = [];
}
