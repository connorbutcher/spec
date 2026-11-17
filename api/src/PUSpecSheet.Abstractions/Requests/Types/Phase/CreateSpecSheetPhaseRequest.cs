using PUSpecSheet.Abstractions.Requests.Access;

namespace PUSpecSheet.Abstractions.Requests.Types.Phase;

public sealed record CreateSpecSheetPhaseRequest
{
    public required int? ParentSpecSheetPhaseId { get; init; }

    public required string Phase { get; init; }

    public required string PhaseShort { get; init; }

    public required int DisplayOrder { get; init; }

    public required ICollection<int> SpecSheetTypeIds { get; init; }

    public required ICollection<CreateSpecSheetPhaseObserverRequest> CreateSpecSheetPhaseObserverRequests { get; init; }
}
