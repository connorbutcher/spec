namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// One version of a <see cref="TableTemplate"/>'s layout. New sheet tables use the latest version; a
/// version becomes read-only once a sheet table uses it, and changes then go into a new version, so
/// sheets keep the layout they were built with.
/// </summary>
public class TableTemplateVersion
{
    public int Id { get; set; }

    public int TableTemplateId { get; set; }

    public TableTemplate TableTemplate { get; set; } = null!;

    /// <summary>1 for the first version, counting up within the template.</summary>
    public int VersionNumber { get; set; }

    public TemplateOrientation Orientation { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    /// <summary>Every section in this version, at all levels of the tree.</summary>
    public ICollection<TemplateSection> Sections { get; set; } = [];
}
