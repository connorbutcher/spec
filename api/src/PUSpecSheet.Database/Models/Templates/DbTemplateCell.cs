using PUSpecSheet.Database.Models.CellTypes;
using PUSpecSheet.Domain.CellTypes.Configurations;
using PUSpecSheet.Domain.CellTypes.Styles;
using PUSpecSheet.Domain.Templates;

namespace PUSpecSheet.Database.Models.Templates;

/// <summary>
/// One cell in a <see cref="DbTemplateRow"/>. The cell starts on its row; <see cref="Column"/> is
/// 1-based, and the spans map directly onto CSS grid-row / grid-column span. In a horizontal table a
/// cell can belong to a <see cref="DbTemplateColumnBlock"/>; its column then counts from the block's first
/// column, and every copy of the block on a sheet gets its own copy of the cell.
/// </summary>
public class DbTemplateCell
{
    public int Id { get; set; }

    public int TemplateRowId { get; set; }

    public DbTemplateRow TemplateRow { get; set; } = null!;

    /// <summary>The column block the cell belongs to, or null for the row's own cells, which come before the blocks.</summary>
    public int? TemplateColumnBlockId { get; set; }

    public DbTemplateColumnBlock? TemplateColumnBlock { get; set; }

    /// <summary>The 1-based column the cell starts on, within its column block when it has one.</summary>
    public int Column { get; set; }

    public int RowSpan { get; set; } = 1;

    public int ColumnSpan { get; set; } = 1;

    public int CellTypeId { get; set; }

    public DbCellType CellType { get; set; } = null!;

    /// <summary>The text of a heading or group cell, or the prompt shown in an input cell.</summary>
    public string? Caption { get; set; }

    /// <summary>Whether the cell must be filled in on a sheet. Ignored for heading and group cells.</summary>
    public bool IsRequired { get; set; }

    /// <summary>
    /// The settings this cell changes from its cell type's <see cref="DbCellType.Configuration"/>, or null
    /// to use them as they are. See <c>CellSettingsResolver</c>.
    /// </summary>
    public CellConfiguration? ConfigurationOverride { get; set; }

    /// <summary>The style values this cell changes from its cell type's <see cref="DbCellType.Style"/>.</summary>
    public CellStyle? StyleOverride { get; set; }

    /// <summary>
    /// The name other applications look this cell up by, such as "partNumber", or null when it can't be
    /// looked up. See <see cref="LookupKeyRules"/>. It is carried into each new version of the template.
    /// </summary>
    public string? LookupKey { get; set; }
}
