using Microsoft.EntityFrameworkCore.Storage;
using PUSpecSheet.Abstractions.Domain.Legacy.SpecSheets;
using PUSpecSheet.Abstractions.Operations;
using PUSpecSheet.Abstractions.Requests.SpecSheets;
using PUSpecSheet.Abstractions.Requests.Workflow;
using PUSpecSheet.Query.Requests;

namespace PUSpecSheet.Api.DAL.DataAccess.SpecSheets;

public interface ISpecSheetDataAccessService
{
    #region Spec Sheets

    Task<SpecSheet> GetSpecSheetByIdAsync(int id, CancellationToken cancellationToken, bool permissionTrimTables = true);

    Task<SpecSheet?> GetLatestPublishedSpecSheetByTypeAndPhaseAsync(
        int typeId,
        int phaseId,
        DateTime? publishedByDateTime,
        CancellationToken cancellationToken);

    Task<(SpecSheet? SpecSheet, IReadOnlyList<SpecSheetTableCell> Cells)> GetLatestPublishedSpecSheetWithCellsByTypeAndPhaseAsync(
        int typeId,
        int phaseId,
        IReadOnlyCollection<Guid> referenceIds,
        CancellationToken cancellationToken);

    Task<(SpecSheet? SpecSheet, IReadOnlyList<SpecSheetTableCell> Cells)> GetPUSpecSheetWithCellsByTypeAndPhaseAsync(
        int puSpecSheetId,
        IReadOnlyCollection<Guid> referenceIds,
        CancellationToken cancellationToken);

    Task<SpecSheetSummary?> GetLatestPublishedSpecSheetSummaryByTypeAndPhaseAsync(
    int typeId,
    int phaseId,
    DateTime? publishedByDateTime,
    CancellationToken cancellationToken);

    Task<IReadOnlyList<SpecSheetSummary>> GetAllSpecSheetsByPhaseAsync(int phaseId, bool includeChildren, CancellationToken cancellationToken);

    Task<IReadOnlyList<SpecSheetSummary>> GetAllSpecSheetsByTypeAsync(int typeId, CancellationToken cancellationToken);

    Task<IReadOnlyList<SpecSheetSummary>> GetAllSpecSheetsByPhaseAndTypeAsync(int phaseId, int typeId, bool? published, CancellationToken cancellationToken);

    [Obsolete("Use ISheetDataAccessService.CreateDraftAsync.")]
    Task<int> CreateSpecSheetAsync(CreateSpecSheetRequest createPUSpecSheetRequest, CancellationToken cancellationToken);

    [Obsolete("Use ISheetDataAccessService.ApplyChangesAsync.")]
    Task UpdateSpecSheetAsync(SpecSheetChanges specSheetChanges, CancellationToken cancellationToken);

    [Obsolete("Use ISheetDataAccessService.PublishAsync.")]
    Task PublishSpecSheetAsync(PublishSpecSheetRequest publishSpecSheetRequest, CancellationToken cancellationToken);

    [Obsolete("Use ISheetDataAccessService.DeleteDraftAsync.")]
    Task DeleteSpecSheetAsync(int id, CancellationToken cancellationToken);

    [Obsolete("Use ISheetDataAccessService.DecideSubmissionAsync.")]
    Task UpdateWorkflowStepActorStateAsync(UpdateWorkflowStepActorRequest updateWorkflowStepActorRequest, CancellationToken cancellationToken);

    [Obsolete("Use ISheetDataAccessService.ReassignSubmissionApproversAsync.")]
    Task ReassignWorkflowStepActorsAsync(ReassignWorkflowStepActorsRequest reassignWorkflowStepActorsRequest, CancellationToken cancellationToken);

    Task UpdateSystemReferencesAsync(ICollection<ExternalSystemReferenceRequest> systemReferences,
        IDbContextTransaction? transactionContext,
        CancellationToken cancellationToken);

    Task DeleteSystemReferencesByExternalIdAsync(int externalId, CancellationToken cancellationToken);

    #endregion Spec Sheets

    #region Tables

    Task<IReadOnlyList<SpecSheetTable>> GetAllUniqueSpecSheetTablesAsync(CancellationToken cancellationToken);

    #endregion Tables
}
