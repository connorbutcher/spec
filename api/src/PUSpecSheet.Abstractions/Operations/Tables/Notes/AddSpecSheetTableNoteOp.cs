using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Operations.Tables.Notes;

public sealed record AddSpecSheetTableNoteOp : IChangeOp
{
    public required int SpecSheetTableId { get; init; }

    [MinLength(1)]
    public required string Note { get; init; }

    [MinLength(1)]
    [MaxLength(16)]
    public required string SourceSystem { get; init; }
}
