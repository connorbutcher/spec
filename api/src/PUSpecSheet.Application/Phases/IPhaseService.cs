using PUSpecSheet.Contracts.Phases;

namespace PUSpecSheet.Application.Phases;

public interface IPhaseService
{
    /// <summary>Every phase as a flat list, ordered by display order then code.</summary>
    Task<IReadOnlyList<PhaseDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<PhaseDto> GetAsync(int id, CancellationToken cancellationToken);

    /// <summary>Adds a phase after its siblings, under a parent or at the top level.</summary>
    Task<PhaseDto> CreateAsync(CreatePhaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Moves a phase to a position under a parent, or at the top level, and renumbers the siblings it
    /// joins and leaves. A phase can't go under itself or anything beneath it. Returns every phase.
    /// </summary>
    Task<IReadOnlyList<PhaseDto>> MoveAsync(int id, MovePhaseRequest request, CancellationToken cancellationToken);

    /// <summary>
    /// Replaces the sheet types a phase has. A type whose sheet already has tables on this phase can't be
    /// removed.
    /// </summary>
    Task<PhaseDto> SetSheetTypesAsync(int id, SetPhaseSheetTypesRequest request, CancellationToken cancellationToken);
}
