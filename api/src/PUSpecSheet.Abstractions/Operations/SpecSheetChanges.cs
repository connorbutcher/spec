using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Operations;

public sealed record SpecSheetChanges
{
    [Range(1, int.MaxValue)]
    public required int SpecSheetId { get; init; }

    [MinLength(1)]
    public required IChangeOp[] Operations { get; init; }
}
