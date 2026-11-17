namespace PUSpecSheet.Abstractions.Operations.Tables;

public sealed record DeleteSpecSheetTableOp : IDeleteOp
{
    public required int Id { get; init; }
}
