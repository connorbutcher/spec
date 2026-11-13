using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Database.Models.Templates;

/// <summary>
/// A named block of a <see cref="DbTableTemplateVersion"/>. Sections form a tree through
/// <see cref="ParentSectionId"/>: the one top-level header, then addable sections with addable
/// sub-sections inside them. A section can hold its own <see cref="DbTemplateRow"/>s of cells and child
/// sections together; its rows come first, then its sub-sections. It maps onto a CSS subgrid.
/// </summary>
public class DbTemplateSection
{
    public int Id { get; set; }

    public int TableTemplateVersionId { get; set; }

    public int? ParentSectionId { get; set; }

    public string Name { get; set; } = string.Empty;

    /// <summary>Order among sibling sections.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>The table's header, or a section people add copies of on a sheet.</summary>
    public SectionRole Role { get; set; } = SectionRole.Addable;

    /// <summary>The fewest copies a sheet table can have. Copies can't be removed below this.</summary>
    public int MinInstances { get; set; }

    /// <summary>The most copies a sheet table can have; null means no limit. Always 1 for the header.</summary>
    public int? MaxInstances { get; set; }

    /// <summary>How many copies a table starts with when it's added to a sheet.</summary>
    public int InitialInstances { get; set; }

    public virtual DbTableTemplateVersion TableTemplateVersion { get; set; } = null!;

    public virtual DbTemplateSection? ParentSection { get; set; }

    public virtual ICollection<DbTemplateSection> ChildSections { get; set; } = [];

    /// <summary>This section's own rows, shown before its child sections.</summary>
    public virtual ICollection<DbTemplateRow> Rows { get; set; } = [];
}
