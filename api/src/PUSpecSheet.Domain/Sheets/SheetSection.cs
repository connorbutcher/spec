using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Domain.Sheets;

/// <summary>
/// A section of a sheet table, laid out from a <see cref="TemplateSection"/>. Sections nest through
/// <see cref="ParentSheetSectionId"/> like the template's, and a template section can appear more than
/// once. Whether it's on the sheet, and where, is versioned in <see cref="Revisions"/>.
/// </summary>
public class SheetSection
{
    public int Id { get; set; }

    /// <summary>Stable identifier that stays with the section across every version.</summary>
    public Guid PublicId { get; set; }

    public int SheetTableId { get; set; }

    public SheetTable SheetTable { get; set; } = null!;

    public int TemplateSectionId { get; set; }

    public TemplateSection TemplateSection { get; set; } = null!;

    public int? ParentSheetSectionId { get; set; }

    public SheetSection? ParentSheetSection { get; set; }

    public ICollection<SheetSection> ChildSheetSections { get; set; } = [];

    public DateTime CreatedAtUtc { get; set; }

    public ICollection<SheetSectionRevision> Revisions { get; set; } = [];

    public ICollection<SheetRow> Rows { get; set; } = [];
}
