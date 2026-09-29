namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// A named block of a <see cref="TableTemplateVersion"/>. Sections form a tree through
/// <see cref="ParentSectionId"/>; a section with no parent sits at the top of the table. A section holds
/// either child sections or <see cref="TemplateRow"/>s of cells, never both, and maps onto a CSS subgrid.
/// </summary>
public class TemplateSection
{
    public int Id { get; set; }

    public int TableTemplateVersionId { get; set; }

    public TableTemplateVersion TableTemplateVersion { get; set; } = null!;

    public int? ParentSectionId { get; set; }

    public TemplateSection? ParentSection { get; set; }

    public ICollection<TemplateSection> ChildSections { get; set; } = [];

    public string Name { get; set; } = string.Empty;

    /// <summary>Order among sibling sections.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>A single block such as a header, or a block people add copies of on a sheet.</summary>
    public SectionRole Role { get; set; } = SectionRole.Fixed;

    /// <summary>The fewest copies a sheet table can have. Copies can't be removed below this.</summary>
    public int MinInstances { get; set; } = 1;

    /// <summary>The most copies a sheet table can have; null means no limit. Always 1 for a fixed section.</summary>
    public int? MaxInstances { get; set; } = 1;

    /// <summary>How many copies a table starts with when it's added to a sheet.</summary>
    public int InitialInstances { get; set; } = 1;

    /// <summary>The rows of this section. Only a section without child sections has rows.</summary>
    public ICollection<TemplateRow> Rows { get; set; } = [];
}
