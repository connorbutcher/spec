using PUSpecSheet.Abstractions.Phases;

namespace PUSpecSheet.DataAccess.Phases;

public interface IPhasesService
{
    Task<IReadOnlyList<Phase>> GetAllAsync(CancellationToken cancellationToken);

    Task<Phase> CreateAsync(CreatePhaseRequest request, CancellationToken cancellationToken);

    Task<IReadOnlyList<Phase>> MoveAsync(int id, MovePhaseRequest request, CancellationToken cancellationToken);

    Task<Phase> SetSheetTypesAsync(int id, SetPhaseSheetTypesRequest request, CancellationToken cancellationToken);
}
