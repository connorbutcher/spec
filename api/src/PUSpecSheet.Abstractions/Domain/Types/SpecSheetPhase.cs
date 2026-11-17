using PUSpecSheet.Abstractions.Domain.Access;

namespace PUSpecSheet.Abstractions.Domain.Types;

public record SpecSheetPhase
{
    public required int Id { get; init; }

    public required int? ParentSpecSheetPhaseId { get; init; }

    public required string Phase { get; init; }

    public required string PhaseShort { get; init; }

    public required bool IsObsolete { get; init; }

    public required int DisplayOrder { get; init; }

    public required ICollection<SpecSheetPhase> Phases { get; set; } = [];

    public required ICollection<SpecSheetType> Types { get; init; }

    public required ICollection<SpecSheetPhaseObserver> PhaseObservers { get; init; }
}
