using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Database.Models.Templates;

/// <summary>
/// One version of a <see cref="DbTableTemplate"/>'s layout. New sheet tables use the latest version; a
/// version becomes read-only once a sheet table uses it, and changes then go into a new version, so
/// sheets keep the layout they were built with.
/// </summary>
public class DbTableTemplateVersion
{
    public int Id { get; set; }

    public int TableTemplateId { get; set; }

    /// <summary>1 for the first version, counting up within the template.</summary>
    public int VersionNumber { get; set; }

    public TemplateOrientation Orientation { get; set; }

    /// <summary>
    /// How many of the table's own columns, from the left, stay pinned while the rest scroll sideways: 1 pins
    /// just the first (usually the heading column). Column blocks have their own count.
    /// </summary>
    public int StickyColumnCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public virtual DbTableTemplate TableTemplate { get; set; } = null!;

    /// <summary>Every section in this version, at all levels of the tree.</summary>
    public virtual ICollection<DbTemplateSection> Sections { get; set; } = [];

    /// <summary>The column blocks people add copies of across a horizontal table. Vertical tables have none.</summary>
    public virtual ICollection<DbTemplateColumnBlock> ColumnBlocks { get; set; } = [];
}
