using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Operations.Rows;

public sealed record DeleteSpecSheetTableRowOp : IDeleteOp
{
    [Range(1, int.MaxValue)]
    public required int Id { get; init; }
}