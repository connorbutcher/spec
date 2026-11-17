using System.ComponentModel.DataAnnotations;

namespace PUSpecSheet.Abstractions.Operations.TableGroups;

public sealed record AddSpecSheetTableGroupOp : IChangeOp
{
    [Range(int.MinValue, 1)]
    public required int TempId { get; init; }

    public required int SpecSheetTableId { get; init; }

    [Range(1, int.MaxValue)]
    public required int TableGroupTemplateId { get; init; }

    public int? LinkedSpecSheetTableGroupId { get; init; }

    [Range(1, int.MaxValue)]
    public required int Order { get; init; }

    public required Guid ReferenceId { get; init; }
}
