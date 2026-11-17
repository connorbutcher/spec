using System.ComponentModel.DataAnnotations;
using PUSpecSheet.Abstractions.Domain.Common;

namespace PUSpecSheet.Abstractions.Operations.Values.Text;

public sealed record UpdateTextValueOp : IChangeOp
{
    [Range(1, int.MaxValue)]
    public required int Id { get; init; }

    public PropertyChanged<string?>? Value { get; init; }
}
