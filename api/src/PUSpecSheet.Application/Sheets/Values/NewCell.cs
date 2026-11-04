using PUSpecSheet.Domain.CellTypes;

namespace PUSpecSheet.Application.Sheets.Values;

/// <summary>A cell of a newly added row, with the kind of its cell type.</summary>
internal sealed record NewCell(int Id, int SheetRowId, int TemplateCellId, int? SheetColumnBlockId, CellKind Kind);
