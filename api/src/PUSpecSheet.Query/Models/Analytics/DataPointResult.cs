using PUSpecSheet.Query.Models.Properties;
using PUSpecSheet.Query.Models.Values;

namespace PUSpecSheet.Query.Models.Analytics;

public record DataPointResult
{
    public required int PUSpecSheetTypeId { get; init; }

    public required int PUSpecSheetPhaseId { get; init; }

    public required string DataPointId { get; init; }

    public required Guid ReferenceId { get; init; }

    public required string DisplayName { get; init; }

    public required SlimValue Value { get; init; }

    public required ICollection<SlimPUSpecSheetPropertyValueSummary> Properties { get; init; }
}
