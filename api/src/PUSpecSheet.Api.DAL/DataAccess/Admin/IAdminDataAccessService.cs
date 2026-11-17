using PUSpecSheet.Abstractions.Domain.Access;
using PUSpecSheet.Abstractions.Domain.Revisions;
using PUSpecSheet.Abstractions.Requests.Access;
using PUSpecSheet.Abstractions.Requests.Access.Tables;

namespace PUSpecSheet.Api.DAL.DataAccess.Admin;

public interface IAdminDataAccessService
{
    Task<IReadOnlyList<User>> GetAllUsersAsync(CancellationToken cancellationToken);

    Task<int> CreateUserAsync(CreateUserRequest createUserRequest, CancellationToken cancellationToken);

    Task<IReadOnlyList<Permission>> GetPermissions();

    Task<IReadOnlyList<SpecSheetTablePermission>> GetTablePermissionsByReferenceIdAsync(Guid referenceId, CancellationToken cancellationToken);

    Task UpdateUserPermissionsAsync(UpdateUserPermissionsRequest updateUserPermissionsRequest, CancellationToken cancellationToken);

    Task UpdateTablePermissionsAsync(ManageTablePermissionsRequest manageTablePermissionsRequest, CancellationToken cancellationToken);

    Task DeleteTablePermissionsByReferenceIdAsync(Guid referenceId, CancellationToken cancellationToken);

    Task RefreshCacheForSpecSheetTypeAsync(int specSheetTypeId, CancellationToken cancellationToken);

    Task ResetCacheForSpecSheetTypeAsync(int specSheetTypeId, CancellationToken cancellationToken);

    /// <summary>
    /// Reads every published spec sheet (of a type or phase, when given) from its copy and from the revisions, and
    /// compares them. Nothing is cached or changed.
    /// </summary>
    Task<RevisionReadCheck> CompareRevisionReadsAsync(int? specSheetTypeId, int? specSheetPhaseId, CancellationToken cancellationToken);
}
