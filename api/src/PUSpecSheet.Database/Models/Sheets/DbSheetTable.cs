using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// A table on a sheet, laid out from a <see cref="DbTableTemplateVersion"/> of the sheet's type. Its title,
/// position and whether it's on the sheet are versioned in <see cref="Revisions"/>.
/// </summary>
public class DbSheetTable
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the table across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetId { get; set; }

    /// <summary>The template version the table was built from. The table keeps this layout.</summary>
    public int TableTemplateVersionId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual DbSheet Sheet { get; set; } = null!;

    public virtual DbTableTemplateVersion TableTemplateVersion { get; set; } = null!;

    public virtual ICollection<DbSheetTableRevision> Revisions { get; set; } = [];

    public virtual ICollection<DbSheetSection> Sections { get; set; } = [];

    /// <summary>The copies of the template's column blocks, for a horizontal table.</summary>
    public virtual ICollection<DbSheetColumnBlock> ColumnBlocks { get; set; } = [];
}
