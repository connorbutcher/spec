namespace PUSpecSheet.Abstractions.Operations.Tables.Notes;

public sealed record UpdateSpecSheetTableNoteOp : IChangeOp
{
    public required int Id { get; init; }

    public bool? IsObsolete { get; init; }
}
