using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Query.Models.Properties;

public sealed record SlimPUSpecSheetPropertyValueSummary
{
    public required string PropertyKey { get; init; }

    public required string PropertyName { get; init; }

    public required Guid ReferenceId { get; init; }

    public required string ValueControlType { get; init; }

    public required SlimValue Value { get; init; }
}
