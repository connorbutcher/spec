namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// One row of a leaf <see cref="TemplateSection"/>. Rows are laid out in <see cref="DisplayOrder"/>
/// and map onto the section's CSS subgrid rows.
/// </summary>
public class TemplateRow
{
    public int Id { get; set; }

    public int TemplateSectionId { get; set; }

    public TemplateSection TemplateSection { get; set; } = null!;

    /// <summary>Order among the rows of the same section.</summary>
    public int DisplayOrder { get; set; }

    public ICollection<TemplateCell> Cells { get; set; } = [];
}
