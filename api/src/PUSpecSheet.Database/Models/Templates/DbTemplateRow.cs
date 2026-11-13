namespace PUSpecSheet.Database.Models.Templates;

/// <summary>
/// One row of a leaf <see cref="DbTemplateSection"/>. Rows are laid out in <see cref="DisplayOrder"/>
/// and map onto the section's CSS subgrid rows.
/// </summary>
public class DbTemplateRow
{
    public int Id { get; set; }

    public int TemplateSectionId { get; set; }

    /// <summary>Order among the rows of the same section.</summary>
    public int DisplayOrder { get; set; }

    public virtual DbTemplateSection TemplateSection { get; set; } = null!;

    public virtual ICollection<DbTemplateCell> Cells { get; set; } = [];
}
