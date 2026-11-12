using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a cell to a row, among the row's own cells or, with <see cref="TemplateColumnBlockId"/>, to that
/// column block's cells in the row. With <see cref="Column"/> the cell goes in at that column and the
/// cells from there on move one column right; without it the cell goes after the last one. Without a
/// <see cref="CellTypeId"/> the cell uses the first Text cell type.
/// </summary>
public sealed record CreateTemplateCellRequest
{
    public CreateTemplateCellRequest(
        int templateRowId,
        int? cellTypeId = null,
        [Range(1, 100)] int? column = null,
        int? templateColumnBlockId = null)
    {
        this.TemplateRowId = templateRowId;
        this.CellTypeId = cellTypeId;
        this.Column = column;
        this.TemplateColumnBlockId = templateColumnBlockId;
    }

    public int TemplateRowId { get; init; }

    public int? CellTypeId { get; init; }

    public int? Column { get; init; }

    public int? TemplateColumnBlockId { get; init; }
}
