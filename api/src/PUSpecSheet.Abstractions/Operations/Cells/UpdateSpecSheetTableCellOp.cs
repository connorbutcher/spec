using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Domain.Common;
using PUSpecSheet.Abstractions.Domain.Properties.Cell;

namespace PUSpecSheet.Abstractions.Operations.Cells;

public sealed record UpdateSpecSheetTableCellOp : IChangeOp
{
    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    public int? RowNumber { get; init; }

    public int? ColumnNumber { get; init; }

    public PropertyChanged<ICellProperties?>? ConfigurationOverride { get; init; }
}
