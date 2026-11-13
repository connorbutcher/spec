namespace PUSpecSheet.Database.Models.Templates;

/// <summary>
/// A block of columns in a horizontal <see cref="DbTableTemplateVersion"/> that people add copies of on a
/// sheet, side by side to the right of the rows' own cells. A block has no rows of its own: every row of
/// the table, in the header and in the addable sections, holds the block's cells for that row (the
/// <see cref="DbTemplateCell"/>s whose <see cref="DbTemplateCell.TemplateColumnBlockId"/> is this block), so
/// each copy of the block runs the full height of the table.
/// </summary>
public class DbTemplateColumnBlock
{
    public int Id { get; set; }

    public int TableTemplateVersionId { get; set; }

    public DbTableTemplateVersion TableTemplateVersion { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    /// <summary>Order among the version's column blocks, left to right.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>The fewest copies a sheet table can have. Copies can't be removed below this.</summary>
    public int MinInstances { get; set; }

    /// <summary>The most copies a sheet table can have; null means no limit.</summary>
    public int? MaxInstances { get; set; }

    /// <summary>How many copies a table starts with when it's added to a sheet.</summary>
    public int InitialInstances { get; set; }

    /// <summary>
    /// How many of the block's columns, from its left, stay pinned beside the table's own sticky columns once
    /// scrolled to. 0 leaves the whole block free to scroll.
    /// </summary>
    public int StickyColumnCount { get; set; }

    public ICollection<DbTemplateCell> Cells { get; set; } = [];
}
