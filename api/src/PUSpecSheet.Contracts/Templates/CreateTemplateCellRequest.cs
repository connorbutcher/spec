using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a cell to a row, among the row's own cells or, with <see cref="TemplateColumnBlockId"/>, to that
/// column block's cells in the row. With <see cref="Column"/> the cell goes in at that column and the
/// cells from there on move one column right; without it the cell goes after the last one. Without a
/// <see cref="CellTypeId"/> the cell uses the first Text cell type.
/// </summary>
public sealed record CreateTemplateCellRequest(
    int TemplateRowId,
    int? CellTypeId = null,
    [Range(1, 100)] int? Column = null,
    int? TemplateColumnBlockId = null);
