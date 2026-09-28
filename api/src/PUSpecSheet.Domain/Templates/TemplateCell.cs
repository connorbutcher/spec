using PUSpecSheet.Domain.CellTypes;

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

    public int CellTypeId { get; set; }

    public CellType CellType { get; set; } = null!;

    /// <summary>The text of a label cell, or the prompt shown in an input cell.</summary>
    public string? Caption { get; set; }

    /// <summary>Whether the cell must be filled in on a sheet. Ignored for label cells.</summary>
    public bool IsRequired { get; set; }
}
