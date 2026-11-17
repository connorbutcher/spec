namespace PUSpecSheet.Abstractions.Operations.Cells;

public sealed record DeleteSpecSheetTableCellOp : IDeleteOp
{
    public required int Id { get; init; }
}
