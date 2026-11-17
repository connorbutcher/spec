using PUSpecSheet.Abstractions.Domain.Types;
using PUSpecSheet.Abstractions.Requests.Types.Phase;
using PUSpecSheet.Abstractions.Requests.Types.Type;

namespace PUSpecSheet.Api.DAL.DataAccess.Types;

public interface ITypeDataAccessService
{
    #region Spec Sheet Types

    Task<ICollection<SpecSheetType>> GetAllSpecSheetTypesAsync(CancellationToken cancellationToken);

    Task<SpecSheetType> GetSpecSheetTypeByIdAsync(int id, CancellationToken cancellationToken);

    Task CreateSpecSheetTypeAsync(CreateSpecSheetTypeRequest createSpecSheetTypeRequest, CancellationToken cancellationToken);

    Task UpdateSpecSheetTypeAsync(UpdateSpecSheetTypeRequest updateSpecSheetTypeRequest, CancellationToken cancellationToken);

    #endregion Spec Sheet Types    

    #region Spec Sheet Phases

    Task<IReadOnlyList<SpecSheetPhase>> GetAllSpecSheetPhasesAsync(CancellationToken cancellationToken);

    Task CreateSpecSheetPhaseAsync(CreateSpecSheetPhaseRequest createSpecSheetPhaseRequest, CancellationToken cancellationToken);

    Task UpdateSpecSheetPhaseAsync(UpdateSpecSheetPhaseRequest updateSpecSheetPhaseRequest, CancellationToken cancellationToken);

    Task UpdateManySpecSheetPhasesAsync(IReadOnlyList<UpdateSpecSheetPhaseRequest> updateRequests, CancellationToken cancellationToken);

    #endregion Spec Sheet Phases
}
