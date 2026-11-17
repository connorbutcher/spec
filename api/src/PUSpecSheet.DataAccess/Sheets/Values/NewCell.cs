using PUSpecSheet.Abstractions.CellTypes;

namespace PUSpecSheet.DataAccess.Sheets.Values;

/// <summary>A cell of a newly added row, with the kind of its cell type.</summary>
public sealed record NewCell
{
    public required int Id { get; init; }

    public required int SheetRowId { get; init; }

    public required int TemplateCellId { get; init; }

    public int? SheetColumnBlockId { get; init; }

    public required CellKind Kind { get; init; }
}
