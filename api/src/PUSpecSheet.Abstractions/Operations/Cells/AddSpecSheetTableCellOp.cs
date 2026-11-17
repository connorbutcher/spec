using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;

namespace PUSpecSheet.Abstractions.Operations.Cells;

public sealed record AddSpecSheetTableCellOp : IChangeOp
{
    [Range(int.MinValue, 1)]
    public required int TempId { get; init; }

    public required int SpecSheetTableRowId { get; init; }

    [Range(1, int.MaxValue)]
    public required int TableCellTypeId { get; init; }


    [Range(1, int.MaxValue)]
    public required int CellConfigurationId { get; init; }

    [Range(1, int.MaxValue)]
    public required int RowNumber { get; init; }

    [Range(1, int.MaxValue)]
    public required int ColumnNumber { get; init; }

    [Range(1, int.MaxValue)]
    public required int RowSpan { get; init; }

    [Range(1, int.MaxValue)]
    public required int ColSpan { get; init; }

    public required Guid ReferenceId { get; init; }

    public ICellProperties? ConfigurationOverride { get; init; }
}
