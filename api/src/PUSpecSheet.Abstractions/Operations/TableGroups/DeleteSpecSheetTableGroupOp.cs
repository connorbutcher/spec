namespace PUSpecSheet.Abstractions.Operations.TableGroups;

public sealed record DeleteSpecSheetTableGroupOp : IDeleteOp
{
    public required int Id { get; init; }
}
