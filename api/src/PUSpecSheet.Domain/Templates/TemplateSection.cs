namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// A named block of a <see cref="TableTemplate"/>. Sections form a tree through
/// <see cref="ParentSectionId"/>; a section with no parent sits at the top of the template.
/// Leaf sections hold <see cref="TemplateRow"/>s of cells and map onto a CSS subgrid.
/// </summary>
public class TemplateSection
{
    public int Id { get; set; }

    public int TableTemplateId { get; set; }

    public TableTemplate TableTemplate { get; set; } = null!;

    public int? ParentSectionId { get; set; }

    public TemplateSection? ParentSection { get; set; }

    public ICollection<TemplateSection> ChildSections { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    /// <summary>Order among sibling sections.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Whether the section comes with the table on a sheet, and whether it can be removed there.</summary>
    public SectionInclusion Inclusion { get; set; } = SectionInclusion.Default;

    /// <summary>The rows of this section. Only leaf sections are expected to have rows.</summary>
    public ICollection<TemplateRow> Rows { get; set; } = [];
}
