namespace PUSpecSheet.Contracts.Templates;

/// <summary>
/// Adds a cell after the last cell of a row. Without a <see cref="CellTypeId"/> the cell uses the
/// first Text cell type.
/// </summary>
public sealed record CreateTemplateCellRequest(int TemplateRowId, int? CellTypeId);
