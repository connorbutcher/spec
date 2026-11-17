using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Abstractions.Operations.Tables;

public record AddSpecSheetTableOp : IChangeOp
{
    [Range(int.MinValue, 1)]
    public required int TempId { get; init; }

    public required int SpecSheetId { get; init; }

    [Range(1, int.MaxValue)]
    public required int TableTemplateId { get; init; }

    [MaxLength(64)]
    [MinLength(1)]
    public required string Title { get; init; }

    [Range(1, int.MaxValue)]
    public required int DisplayOrder { get; init; }

    public required Guid ReferenceId { get; init; }

    public ITableProperties? ConfigurationOverride { get; init; }
}
