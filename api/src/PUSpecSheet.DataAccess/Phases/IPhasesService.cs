using PUSpecSheet.Abstractions.Phases;

namespace PUSpecSheet.DataAccess.Phases;

public interface IPhasesService
{
    Task<IReadOnlyList<PhaseDto>> GetAllAsync(CancellationToken cancellationToken);

    Task<PhaseDto> CreateAsync(CreatePhaseRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<PhaseDto>> MoveAsync(int id, MovePhaseRequest request, CancellationToken cancellationToken);

    Task<PhaseDto> SetSheetTypesAsync(int id, SetPhaseSheetTypesRequest request, CancellationToken cancellationToken);
}
