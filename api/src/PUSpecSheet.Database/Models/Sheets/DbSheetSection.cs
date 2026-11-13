using PUSpecSheet.Database.Models.Templates;

namespace PUSpecSheet.Database.Models.Sheets;

/// <summary>
/// A section of a sheet table, laid out from a <see cref="DbTemplateSection"/>. Sections nest through
/// <see cref="ParentSheetSectionId"/> like the template's, and a template section can appear more than
/// once. Whether it's on the sheet, and where, is versioned in <see cref="Revisions"/>.
/// </summary>
public class DbSheetSection
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the section across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetTableId { get; set; }

    public int TemplateSectionId { get; set; }

    public int? ParentSheetSectionId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual DbSheetTable SheetTable { get; set; } = null!;

    public virtual DbTemplateSection TemplateSection { get; set; } = null!;

    public virtual DbSheetSection? ParentSheetSection { get; set; }

    public virtual ICollection<DbSheetSection> ChildSheetSections { get; set; } = [];

    public virtual ICollection<DbSheetSectionRevision> Revisions { get; set; } = [];

    public virtual ICollection<DbSheetRow> Rows { get; set; } = [];
}
