namespace PUSpecSheet.Database;

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

    public DbSheetTable SheetTable { get; set; } = null!;

    public int TemplateSectionId { get; set; }

    public DbTemplateSection TemplateSection { get; set; } = null!;

    public int? ParentSheetSectionId { get; set; }

    public DbSheetSection? ParentSheetSection { get; set; }

    public ICollection<DbSheetSection> ChildSheetSections { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<DbSheetSectionRevision> Revisions { get; set; } = [];

    public ICollection<DbSheetRow> Rows { get; set; } = [];
}
