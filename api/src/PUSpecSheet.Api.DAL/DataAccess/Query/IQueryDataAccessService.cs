using PUSpecSheet.Query.Models;
using PUSpecSheet.Query.Models.Analytics;
using PUSpecSheet.Query.Models.Phase;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.DAL.DataAccess.Query;

public interface IQueryDataAccessService
{
    Task<ICollection<SlimPUSpecSheetType>> GetAllSpecSheetTypesAsync(CancellationToken cancellationToken);

    Task<ICollection<SlimSpecSheetPhaseWithChildren>> GetAllSpecSheetPhasesAsync(CancellationToken cancellationToken);

    Task<SlimPUSpecSheet> GetSpecSheetByIdAsync(int specSheetId, CancellationToken cancellationToken);

    Task<ICollection<SlimPUSpecSheetSummary>> GetAllSpecSheetsByTypeAsync(int typeId, CancellationToken cancellationToken);

    Task<ICollection<SlimPUSpecSheetSummary>> GetAllSpecSheetsByTypeAndPhaseAsync(int typeId, int phaseId, CancellationToken cancellationToken);

    Task<SlimPUSpecSheet> GetLatestPublishedSpecSheetByPhaseAndTypeAsync(int typeId,
        int phaseId,
        DateTime? publishedByDateTime,
        CancellationToken cancellationToken);

    Task<ICollection<SlimPUSpecSheetSummary>> GetPublishedSpecSheetsByPhaseAndTypeAfterDateAsync(int typeId, int phaseId, DateTime publishedAfterDateTime, CancellationToken cancellationToken);

    Task<ICollection<DataPointResult>> GetDatasetDataAsync(DatasetRequest datasetRequest, CancellationToken cancellationToken);

    Task UpdateSystemReferencesAsync(ICollection<ExternalSystemReferenceRequest> systemReferences, CancellationToken cancellationToken);

    Task DeleteSystemReferencesByExternalIdAsync(int externalId, CancellationToken cancellationToken);
}
