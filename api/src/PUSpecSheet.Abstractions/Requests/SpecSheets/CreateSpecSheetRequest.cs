using PUSpecSheet.Abstractions.Requests.SpecSheets.Properties;

namespace PUSpecSheet.Abstractions.Requests.SpecSheets;

public sealed record CreateSpecSheetRequest
{
    public required int SpecSheetTypeId { get; init; }

    public required int SpecSheetPhaseId { get; init; }

    public required ICollection<CreateSpecSheetPropertyValueRequest> PropertyValues { get; init; }
}
