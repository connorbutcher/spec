namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// A named block of a <see cref="TableTemplate"/>. Sections form a tree through
/// <see cref="ParentSectionId"/>; a section with no parent sits at the top of the template.
/// Leaf sections hold the <see cref="TemplateCell"/>s and map onto a CSS subgrid.
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

    /// <summary>The cells of this section. Only leaf sections are expected to have cells.</summary>
    public ICollection<TemplateCell> Cells { get; set; } = [];
}
