namespace PUSpecSheet.Domain.Templates;

/// <summary>
/// One cell in a <see cref="TemplateRow"/>. The cell starts on its row; <see cref="Column"/> is
/// 1-based, and the spans map directly onto CSS grid-row / grid-column span.
/// </summary>
public class TemplateCell
{
    public int Id { get; set; }

    public int TemplateRowId { get; set; }

    public TemplateRow TemplateRow { get; set; } = null!;

    /// <summary>The 1-based column the cell starts on.</summary>
    public int Column { get; set; }

    public int RowSpan { get; set; } = 1;

    public int ColumnSpan { get; set; } = 1;
}
