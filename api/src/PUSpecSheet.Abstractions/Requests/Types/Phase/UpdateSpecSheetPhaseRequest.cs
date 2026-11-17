using PUSpecSheet.Abstractions.Requests.Access;

namespace PUSpecSheet.Abstractions.Requests.Types.Phase;

public sealed record UpdateSpecSheetPhaseRequest
{
    public required int Id { get; init; }

    public required string Phase { get; init; }

    public required string PhaseShort { get; init; }

    public required int DisplayOrder { get; init; }

    public required bool IsObsolete { get; init; }

    public int? ParentSpecSheetPhaseId { get; init; }

    public required ICollection<int> NewSpecSheetTypeIds { get; init; }

    public required ICollection<int> RemovedSpecSheetTypeIds { get; init; }

    public required ICollection<CreateSpecSheetPhaseObserverRequest> CreateSpecSheetPhaseObserverRequests { get; init; }

    public required ICollection<UpdateSpecSheetPhaseObserverRequest> UpdateSpecSheetPhaseObserverRequests { get; init; }

    public required ICollection<int> RemoveSpecSheetPhaseObserverIds { get; init; }
}
