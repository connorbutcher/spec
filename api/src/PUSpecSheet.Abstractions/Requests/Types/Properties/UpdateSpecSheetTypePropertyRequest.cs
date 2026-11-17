using PUSpecSheet.Abstractions.Domain.Common;
using PUSpecSheet.Abstractions.Domain.Types;

namespace PUSpecSheet.Abstractions.Requests.Types.Properties;

public sealed record UpdateSpecSheetTypePropertyRequest
{
    public int Id { get; init; }

    public string? ValueName { get; init; }

    public string? ValueKey { get; init; }

    public string? ValueType { get; init; }

    public PropertyChanged<string?>? PlaceholderText { get; init; }

    public string? ValueControlType { get; init; }

    public bool? CanValueBeSetInForm { get; init; }

    public int? DisplayOrder { get; init; }

    public PropertyChanged<ICollection<SpecSheetTypePropertyValidator>?>? Validators { get; init; }
}
