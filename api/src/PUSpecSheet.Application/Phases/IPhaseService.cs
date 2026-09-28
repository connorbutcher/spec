using PUSpecSheet.Contracts.Phases;

namespace PUSpecSheet.Application.Phases;

public interface IPhaseService
{
    /// <summary>Every phase as a flat list, ordered by display order then code.</summary>
    Task<IReadOnlyList<PhaseDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<PhaseDto> GetAsync(int id, CancellationToken cancellationToken);
}
