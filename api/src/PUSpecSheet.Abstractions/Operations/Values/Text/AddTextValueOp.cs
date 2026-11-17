namespace PUSpecSheet.Abstractions.Operations.Values.Text;

public sealed record AddTextValueOp : IChangeOp
{
    public required int SpecSheetTableCellId { get; init; }

    public string? Value { get; init; }
}
