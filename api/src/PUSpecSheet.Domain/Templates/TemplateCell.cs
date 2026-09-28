namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// One cell in a leaf <see cref="TemplateSection"/>. Positions are 1-based within the section and
/// map directly onto CSS grid-row / grid-column start and span.
/// </summary>
public class TemplateCell
{
    public int Id { get; set; }

    public int TemplateSectionId { get; set; }

    public TemplateSection TemplateSection { get; set; } = null!;

    /// <summary>The 1-based row the cell starts on.</summary>
    public int Row { get; set; }

    /// <summary>The 1-based column the cell starts on.</summary>
    public int Column { get; set; }

    public int RowSpan { get; set; } = 1;

    public int ColumnSpan { get; set; } = 1;
}
