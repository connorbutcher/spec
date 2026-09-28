using PUSpecSheet.Domain.SheetTypes;

namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// The layout of one table on a sheet type, made up of a tree of <see cref="TemplateSection"/>s.
/// </summary>
public class TableTemplate
{
    public int Id { get; set; }

    public int SheetTypeId { get; set; }

    public SheetType SheetType { get; set; } = null!;

    /// <summary>The template name, unique within its sheet type.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Order among the templates of the same sheet type.</summary>
    public int DisplayOrder { get; set; }

    public TemplateOrientation Orientation { get; set; }

    /// <summary>Every section in the template, at all levels of the tree.</summary>
    public ICollection<TemplateSection> Sections { get; set; } = [];
}
