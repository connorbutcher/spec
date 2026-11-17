using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Operations.Rows;

public sealed record UpdateSpecSheetTableRowOp : IChangeOp
{
    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    public int? RowNumber { get; init; }
}
