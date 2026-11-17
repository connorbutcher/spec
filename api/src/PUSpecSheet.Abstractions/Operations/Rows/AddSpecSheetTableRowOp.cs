using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Operations.Rows;

public sealed record AddSpecSheetTableRowOp : IChangeOp
{
    [Range(int.MinValue, 1)]
    public required int TempId { get; init; }

    public required int SpecSheetTableGroupId { get; init; }

    [Range(1, int.MaxValue)]
    public required int TableRowTemplateId { get; init; }

    [Range(1, int.MaxValue)]
    public required int RowNumber { get; init; }

    public required Guid ReferenceId { get; init; }
}

