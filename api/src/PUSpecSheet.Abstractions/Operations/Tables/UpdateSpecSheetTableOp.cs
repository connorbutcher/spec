using PUSpecSheet.Abstractions.Domain.Common;
using PUSpecSheet.Abstractions.Domain.Properties.Table;

namespace PUSpecSheet.Abstractions.Operations.Tables;

public sealed record UpdateSpecSheetTableOp : IChangeOp
{
    public required int Id { get; init; }

    public string? Title { get; init; }

    public int? DisplayOrder { get; init; }

    public PropertyChanged<ITableProperties?>? ConfigurationOverride { get; init; }
}
